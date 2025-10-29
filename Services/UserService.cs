using Microsoft.EntityFrameworkCore;
using PRM_Backend.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;

namespace PRM_Backend.Services;

public class UserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Helper methods
    private string GenerateFrontendCompatibleToken(object tokenObj)
    {
        var json = JsonSerializer.Serialize(tokenObj);
        var escaped = Uri.EscapeDataString(json);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(escaped));
    }

    private object ToResponseUser(User u) => new
    {
        id = u.Id,
        fullname = u.Fullname,
        email = u.Email,
        role = u.Role,
        phone = u.Phone,
        dob = u.Dob?.ToString("yyyy-MM-dd"),
        address = u.Address
    };

    // User endpoints
    public async Task<IResult> RegisterAsync(RegisterRequest req)
    {
        // Validate input
        var validationResult = ValidateRegisterRequest(req);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new { message = validationResult.ErrorMessage });
        }

        var exists = await _dbContext.Users.AnyAsync(u => u.Email == req.email);
        if (exists) return Results.BadRequest(new { message = "Email đã tồn tại" });

        // Parse date safely
        DateOnly? dobParsed = null;
        if (!string.IsNullOrWhiteSpace(req.dob))
        {
            if (!DateOnly.TryParseExact(req.dob, new[] { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" }, out var dob))
            {
                return Results.BadRequest(new { message = "Định dạng ngày sinh không hợp lệ. Sử dụng yyyy-MM-dd, dd/MM/yyyy hoặc MM/dd/yyyy" });
            }
            dobParsed = dob;
        }

        var user = new User
        {
            Username = req.email!,
            Fullname = req.fullname ?? "new_user",
            Email = req.email!,
            Password = BCrypt.Net.BCrypt.HashPassword(req.password!),
            Phone = req.phone,
            Dob = dobParsed,
            Address = req.address,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var token = GenerateFrontendCompatibleToken(new
        {
            userId = user.Id,
            fullname = user.Fullname,
            email = user.Email,
            role = user.Role,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            type = "temporary",
            source = "register_response"
        });

        return Results.Ok(new { token, user = ToResponseUser(user) });
    }

    public async Task<IResult> LoginAsync(LoginRequest req)
    {
        var idf = (req.identifier ?? string.Empty).Trim();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
            u.Email == idf || u.Phone == idf || u.Username == idf
        );

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.password, user.Password))
        {
            return Results.BadRequest(new { message = "Thông tin đăng nhập không đúng" });
        }

        var token = GenerateFrontendCompatibleToken(new
        {
            userId = user.Id,
            fullname = user.Fullname,
            email = user.Email,
            role = user.Role,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            type = "temporary",
            source = "login_response"
        });

        return Results.Ok(new { token, user = ToResponseUser(user) });
    }

    public async Task<IResult> GetUserAsync(int userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user is null ? Results.NotFound(new { message = "User không tồn tại" }) : Results.Ok(ToResponseUser(user));
    }

    public async Task<IResult> UpdateUserAsync(int userId, Dictionary<string, object> update)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return Results.NotFound(new { message = "User không tồn tại" });

        if (update.TryGetValue("fullname", out var fullname)) user.Fullname = fullname?.ToString() ?? user.Fullname;
        if (update.TryGetValue("phone", out var phone)) 
        {
            var phoneStr = phone?.ToString();
            if (!string.IsNullOrWhiteSpace(phoneStr) && !IsValidPhoneNumber(phoneStr))
            {
                return Results.BadRequest(new { message = "Số điện thoại không hợp lệ (10-11 số, bắt đầu 0)" });
            }
            user.Phone = phoneStr ?? user.Phone;
        }
        if (update.TryGetValue("dob", out var dobStr))
        {
            var s = dobStr?.ToString();
            if (!string.IsNullOrWhiteSpace(s))
            {
                if (!DateOnly.TryParseExact(s, new[] { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" }, out var dob))
                {
                    return Results.BadRequest(new { message = "Định dạng ngày sinh không hợp lệ. Sử dụng yyyy-MM-dd, dd/MM/yyyy hoặc MM/dd/yyyy" });
                }
                user.Dob = dob;
            }
            else
            {
                user.Dob = null;
            }
        }
        if (update.TryGetValue("address", out var address)) user.Address = address?.ToString() ?? user.Address;

        await _dbContext.SaveChangesAsync();
        return Results.Ok(ToResponseUser(user));
    }

    public async Task<IResult> DeleteUserAsync(int userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return Results.NotFound(new { message = "User không tồn tại" });

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(new { success = true });
    }

    public async Task<IResult> GetAllUsersAsync()
    {
        var users = await _dbContext.Users.ToListAsync();
        return Results.Ok(users.Select(ToResponseUser));
    }

    // Validation methods
    private ValidationResult ValidateRegisterRequest(RegisterRequest req)
    {
        // Check required fields
        if (string.IsNullOrWhiteSpace(req.email))
            return new ValidationResult(false, "Email là bắt buộc");
        
        if (string.IsNullOrWhiteSpace(req.password))
            return new ValidationResult(false, "Mật khẩu là bắt buộc");
        
        if (string.IsNullOrWhiteSpace(req.fullname))
            return new ValidationResult(false, "Họ tên là bắt buộc");

        // Validate email format
        if (!IsValidEmail(req.email))
            return new ValidationResult(false, "Định dạng email không hợp lệ");

        // Validate password
        if (!IsValidPassword(req.password))
            return new ValidationResult(false, "Mật khẩu phải có ít nhất 6 ký tự, chứa ít nhất 1 chữ cái và 1 số");

        // Validate phone if provided
        if (!string.IsNullOrWhiteSpace(req.phone) && !IsValidPhoneNumber(req.phone))
            return new ValidationResult(false, "Số điện thoại không hợp lệ (10-11 số, bắt đầu 0)");

        return new ValidationResult(true, "");
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPassword(string password)
    {
        if (password.Length < 6) return false;
        
        bool hasLetter = password.Any(char.IsLetter);
        bool hasDigit = password.Any(char.IsDigit);
        
        return hasLetter && hasDigit;
    }

    private bool IsValidPhoneNumber(string phone)
    {
        // Vietnam phone format: 10-11 digits, starts with 0
        var phoneRegex = new Regex(@"^0\d{9,10}$");
        return phoneRegex.IsMatch(phone);
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }

    public ValidationResult(bool isValid, string errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }
}

// User models
public class RegisterRequest
{
    public string? fullname { get; set; }
    public string? email { get; set; }
    public string? password { get; set; }
    public string? phone { get; set; }
    public string? dob { get; set; }
    public string? address { get; set; }
}

public class LoginRequest
{
    public string? identifier { get; set; }
    public string? password { get; set; }
}
