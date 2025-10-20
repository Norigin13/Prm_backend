using Microsoft.EntityFrameworkCore;
using PRM_Backend.Data;
using System.Text;
using System.Text.Json;
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
        var exists = await _dbContext.Users.AnyAsync(u => u.Email == req.email);
        if (exists) return Results.BadRequest(new { message = "Email đã tồn tại" });

        var user = new User
        {
            Username = req.email!,
            Fullname = req.fullname ?? "new_user",
            Email = req.email!,
            Password = BCrypt.Net.BCrypt.HashPassword(req.password!),
            Phone = req.phone,
            Dob = string.IsNullOrWhiteSpace(req.dob) ? null : DateOnly.Parse(req.dob!),
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
        if (update.TryGetValue("phone", out var phone)) user.Phone = phone?.ToString() ?? user.Phone;
        if (update.TryGetValue("dob", out var dobStr))
        {
            var s = dobStr?.ToString();
            user.Dob = string.IsNullOrWhiteSpace(s) ? null : DateOnly.Parse(s!);
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
