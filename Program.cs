using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI & Swagger UI
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS cho FE
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDev", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:8080",
                "http://127.0.0.1:8080",
                "http://localhost:5162"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

// EF Core: ưu tiên MySQL nếu có, fallback SQLite
var mySql = builder.Configuration.GetConnectionString("MySql");
var sqlite = builder.Configuration.GetConnectionString("Sqlite");
if (!string.IsNullOrWhiteSpace(mySql))
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseMySql(mySql, ServerVersion.AutoDetect(mySql)));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlite(sqlite ?? "Data Source=ezbuild.db"));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Không bắt buộc redirect HTTPS khi dev
// app.UseHttpsRedirection();
app.UseCors("AllowDev");

// Migrate/EnsureCreated + seed tối thiểu
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new Category { Name = "CPU" },
            new Category { Name = "GPU" },
            new Category { Name = "RAM" },
            new Category { Name = "Storage" },
            new Category { Name = "PSU" },
            new Category { Name = "Case" },
            new Category { Name = "Monitor" },
            new Category { Name = "Keyboard" },
            new Category { Name = "Mouse" },
            new Category { Name = "Headset" },
            new Category { Name = "CPUCooler" },
            new Category { Name = "Mainboard" }
        );
        db.SaveChanges();
    }
    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            Username = "demo",
            Fullname = "Demo User",
            Email = "demo@example.com",
            Password = "123456",
            Phone = "+84999999999",
            Dob = DateOnly.Parse("1990-01-01"),
            Address = "HCM",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

// Helpers
string GenerateFrontendCompatibleToken(object tokenObj)
{
    var json = JsonSerializer.Serialize(tokenObj);
    var escaped = Uri.EscapeDataString(json);
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(escaped));
}

object ToResponseUser(User u) => new
{
    id = u.Id,
    fullname = u.Fullname,
    email = u.Email,
    role = u.Role,
    phone = u.Phone,
    dob = u.Dob?.ToString("yyyy-MM-dd"),
    address = u.Address
};

// Category endpoints
app.MapGet("/api/category", (AppDbContext db) => Results.Ok(db.Categories.ToList()));

// Supplier endpoints (basic CRUD)
app.MapGet("/api/supplier", (AppDbContext db) => Results.Ok(db.Suppliers.ToList()));
app.MapPost("/api/supplier", (Supplier s, AppDbContext db) => { db.Suppliers.Add(s); db.SaveChanges(); return Results.Ok(s); });
app.MapPut("/api/supplier/{id:int}", (int id, Supplier s, AppDbContext db) =>
{
    var f = db.Suppliers.FirstOrDefault(x => x.Id == id);
    if (f is null) return Results.NotFound();
    f.Name = s.Name; f.Website = s.Website; db.SaveChanges(); return Results.Ok(f);
});
app.MapDelete("/api/supplier/{id:int}", (int id, AppDbContext db) =>
{
    var f = db.Suppliers.FirstOrDefault(x => x.Id == id);
    if (f is null) return Results.NotFound();
    db.Suppliers.Remove(f); db.SaveChanges(); return Results.Ok(new { success = true });
});

// Product endpoints
app.MapGet("/api/product", (AppDbContext db) =>
{
    var list = db.Products.AsNoTracking()
        .Include(p => p.Category)
        .Include(p => p.ProductPrices)
        .ToList()
        .Select(p => p.ToDto());
    return Results.Ok(list);
});

app.MapGet("/api/product/{id:int}", (int id, AppDbContext db) =>
{
    var found = db.Products.AsNoTracking()
        .Include(p => p.Category)
        .Include(p => p.ProductPrices)
        .FirstOrDefault(p => p.Id == id);
    return found is null ? Results.NotFound(new { message = "Product not found" }) : Results.Ok(found.ToDto());
});

app.MapPost("/api/product", (ProductCreateUpdateDto dto, AppDbContext db) =>
{
    var category = db.Categories.FirstOrDefault(c => c.Id == dto.category_id) ?? db.Categories.First();
    var product = new Product
    {
        Name = dto.name ?? "Unnamed",
        Brand = dto.brand ?? "Unknown",
        CategoryId = category.Id,
        Category = category,
        ImageUrl1 = dto.image_url1 ?? dto.imageUrl1 ?? string.Empty,
        ProductPrices = (dto.productPrices ?? new List<ProductPriceDto>())
            .Select(pp => new ProductPrice { Price = pp.price }).ToList(),
        SpecsJson = dto.specs is null ? null : JsonSerializer.Serialize(dto.specs),
        Socket = dto.socket,
        TdpWatt = dto.tdpWatt ?? dto.tdp_watt
    };
    db.Products.Add(product);
    db.SaveChanges();
    return Results.Ok(product.ToDto());
});

app.MapPut("/api/product/{id:int}", (int id, ProductCreateUpdateDto dto, AppDbContext db) =>
{
    var found = db.Products.Include(p => p.ProductPrices).FirstOrDefault(p => p.Id == id);
    if (found is null) return Results.NotFound(new { message = "Product not found" });

    if (dto.name is not null) found.Name = dto.name;
    if (dto.brand is not null) found.Brand = dto.brand;
    if (dto.image_url1 is not null || dto.imageUrl1 is not null) found.ImageUrl1 = dto.image_url1 ?? dto.imageUrl1!;
    if (dto.socket is not null) found.Socket = dto.socket;
    if (dto.tdpWatt.HasValue || dto.tdp_watt.HasValue) found.TdpWatt = dto.tdpWatt ?? dto.tdp_watt;
    if (dto.category_id > 0)
    {
        var cat = db.Categories.FirstOrDefault(c => c.Id == dto.category_id);
        if (cat is not null) { found.CategoryId = cat.Id; found.Category = cat; }
    }
    if (dto.productPrices is not null)
    {
        found.ProductPrices = dto.productPrices.Select(pp => new ProductPrice { Price = pp.price }).ToList();
    }
    if (dto.specs is not null) found.SpecsJson = JsonSerializer.Serialize(dto.specs);

    db.SaveChanges();
    return Results.Ok(found.ToDto());
});

app.MapDelete("/api/product/{id:int}", (int id, AppDbContext db) =>
{
    var found = db.Products.FirstOrDefault(p => p.Id == id);
    if (found is null) return Results.NotFound(new { message = "Product not found" });
    db.Products.Remove(found);
    db.SaveChanges();
    return Results.Ok(new { success = true });
});

// Services endpoints
app.MapGet("/api/service", (AppDbContext db) => Results.Ok(db.Services.ToList()));
app.MapPost("/api/service", (ServiceEntity s, AppDbContext db) => { db.Services.Add(s); db.SaveChanges(); return Results.Ok(s); });
app.MapPut("/api/service/{id:int}", (int id, ServiceEntity s, AppDbContext db) =>
{
    var f = db.Services.FirstOrDefault(x => x.Id == id);
    if (f is null) return Results.NotFound();
    f.Name = s.Name; f.Description = s.Description; f.BasePrice = s.BasePrice; f.Unit = s.Unit;
    db.SaveChanges(); return Results.Ok(f);
});
app.MapDelete("/api/service/{id:int}", (int id, AppDbContext db) =>
{
    var f = db.Services.FirstOrDefault(x => x.Id == id);
    if (f is null) return Results.NotFound();
    db.Services.Remove(f); db.SaveChanges(); return Results.Ok(new { success = true });
});

// Service Orders endpoints
app.MapGet("/api/service-order", (AppDbContext db) => Results.Ok(db.ServiceOrders.ToList()));
app.MapPost("/api/service-order", (ServiceOrder s, AppDbContext db) => { db.ServiceOrders.Add(s); db.SaveChanges(); return Results.Ok(s); });
app.MapPut("/api/service-order/{id:int}", (int id, ServiceOrder s, AppDbContext db) =>
{
    var f = db.ServiceOrders.FirstOrDefault(x => x.Id == id);
    if (f is null) return Results.NotFound();
    f.UserId = s.UserId; f.ServiceId = s.ServiceId; f.Status = s.Status; f.ScheduledDate = s.ScheduledDate; f.TotalPrice = s.TotalPrice; f.Notes = s.Notes; f.Address = s.Address; f.CreatedAt = s.CreatedAt;
    db.SaveChanges(); return Results.Ok(f);
});
app.MapDelete("/api/service-order/{id:int}", (int id, AppDbContext db) =>
{
    var f = db.ServiceOrders.FirstOrDefault(x => x.Id == id);
    if (f is null) return Results.NotFound();
    db.ServiceOrders.Remove(f); db.SaveChanges(); return Results.Ok(new { success = true });
});

// Service Feedback endpoints
app.MapGet("/api/service-feedback", (AppDbContext db) => Results.Ok(db.ServiceFeedbacks.ToList()));
app.MapPost("/api/service-feedback", (ServiceFeedback s, AppDbContext db) => { db.ServiceFeedbacks.Add(s); db.SaveChanges(); return Results.Ok(s); });
app.MapPut("/api/service-feedback/{id:int}", (int id, ServiceFeedback s, AppDbContext db) =>
{
    var f = db.ServiceFeedbacks.FirstOrDefault(x => x.Id == id);
    if (f is null) return Results.NotFound();
    f.ServiceOrderId = s.ServiceOrderId; f.UserId = s.UserId; f.Rating = s.Rating; f.Comments = s.Comments;
    db.SaveChanges(); return Results.Ok(f);
});
app.MapDelete("/api/service-feedback/{id:int}", (int id, AppDbContext db) =>
{
    var f = db.ServiceFeedbacks.FirstOrDefault(x => x.Id == id);
    if (f is null) return Results.NotFound();
    db.ServiceFeedbacks.Remove(f); db.SaveChanges(); return Results.Ok(new { success = true });
});
// User/Auth endpoints
app.MapPost("/api/user/register", (RegisterRequest req, AppDbContext db) =>
{
    var exists = db.Users.Any(u => u.Email == req.email);
    if (exists) return Results.BadRequest(new { message = "Email đã tồn tại" });

    var user = new User
    {
        Username = req.email!,
        Fullname = req.fullname ?? "new_user",
        Email = req.email!,
        Password = req.password!,
        Phone = req.phone,
        Dob = string.IsNullOrWhiteSpace(req.dob) ? null : DateOnly.Parse(req.dob!),
        Address = req.address,
        Role = "User",
        CreatedAt = DateTime.UtcNow
    };
    db.Users.Add(user);
    db.SaveChanges();

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
});

app.MapPost("/api/user/login", (LoginRequest req, AppDbContext db) =>
{
    var idf = (req.identifier ?? string.Empty).Trim();
    var user = db.Users.FirstOrDefault(u =>
        u.Email == idf || u.Phone == idf || u.Username == idf
    );
    if (user is null || user.Password != req.password)
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
});

app.MapGet("/api/user/{userId:int}", (int userId, AppDbContext db) =>
{
    var user = db.Users.FirstOrDefault(u => u.Id == userId);
    return user is null ? Results.NotFound(new { message = "User không tồn tại" }) : Results.Ok(ToResponseUser(user));
});

app.MapPut("/api/user/{userId:int}", (int userId, Dictionary<string, object> update, AppDbContext db) =>
{
    var user = db.Users.FirstOrDefault(u => u.Id == userId);
    if (user is null) return Results.NotFound(new { message = "User không tồn tại" });

    if (update.TryGetValue("fullname", out var fullname)) user.Fullname = fullname?.ToString() ?? user.Fullname;
    if (update.TryGetValue("phone", out var phone)) user.Phone = phone?.ToString() ?? user.Phone;
    if (update.TryGetValue("dob", out var dobStr))
    {
        var s = dobStr?.ToString();
        user.Dob = string.IsNullOrWhiteSpace(s) ? null : DateOnly.Parse(s!);
    }
    if (update.TryGetValue("address", out var address)) user.Address = address?.ToString() ?? user.Address;

    db.SaveChanges();
    return Results.Ok(ToResponseUser(user));
});

app.MapDelete("/api/user/{userId:int}", (int userId, AppDbContext db) =>
{
    var user = db.Users.FirstOrDefault(u => u.Id == userId);
    if (user is null) return Results.NotFound(new { message = "User không tồn tại" });
    db.Users.Remove(user);
    db.SaveChanges();
    return Results.Ok(new { success = true });
});

// Demo weatherforecast giữ lại nếu cần
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// DbContext & Entities
class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ServiceEntity> Services => Set<ServiceEntity>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<ServiceFeedback> ServiceFeedbacks => Set<ServiceFeedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table & column mappings to match Spring Boot (snake_case)
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.CategoryId).HasColumnName("category_id");
            e.Property(p => p.Brand).HasColumnName("brand");
            e.Property(p => p.Model).HasColumnName("model");
            e.Property(p => p.SpecsJson).HasColumnName("specs");
            e.Property(p => p.TdpWatt).HasColumnName("tdp_watt");
            e.Property(p => p.Color).HasColumnName("color");
            e.Property(p => p.Size).HasColumnName("size");
            e.Property(p => p.Socket).HasColumnName("socket");
            e.Property(p => p.Capacity).HasColumnName("capacity");
            e.Property(p => p.Type).HasColumnName("type");
            e.Property(p => p.ImageUrl1).HasColumnName("image_url1");
            e.Property(p => p.ImageUrl2).HasColumnName("image_url2");
            e.Property(p => p.ImageUrl3).HasColumnName("image_url3");
            e.Property(p => p.ImageUrl4).HasColumnName("image_url4");
            e.Property(p => p.ImageUrl5).HasColumnName("image_url5");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");

            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<ProductPrice>(e =>
        {
            e.ToTable("product_prices");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.ProductId).HasColumnName("product_id");
            e.Property(p => p.Price).HasColumnName("price");
            e.Property(p => p.SupplierId).HasColumnName("supplier_id");
            e.Property(p => p.SupplierLink).HasColumnName("supplier_link");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(pp => pp.Product)
                .WithMany(p => p.ProductPrices)
                .HasForeignKey(pp => pp.ProductId);

            e.HasOne(pp => pp.Supplier)
                .WithMany(s => s.ProductPrices)
                .HasForeignKey(pp => pp.SupplierId);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Username).HasColumnName("username");
            e.Property(p => p.Fullname).HasColumnName("fullname");
            e.Property(p => p.Email).HasColumnName("email");
            e.Property(p => p.Password).HasColumnName("password");
            e.Property(p => p.Phone).HasColumnName("phone");
            e.Property(p => p.Dob).HasColumnName("dob");
            e.Property(p => p.Address).HasColumnName("address");
            e.Property(p => p.Role).HasColumnName("role");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.ToTable("suppliers");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.Website).HasColumnName("website");
        });

        modelBuilder.Entity<ServiceEntity>(e =>
        {
            e.ToTable("services");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.Description).HasColumnName("description");
            e.Property(p => p.BasePrice).HasColumnName("base_price");
            e.Property(p => p.Unit).HasColumnName("unit");
        });

        modelBuilder.Entity<ServiceOrder>(ServiceOrderMap);
        modelBuilder.Entity<ServiceFeedback>(ServiceFeedbackMap);
    }

    private void ServiceOrderMap(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ServiceOrder> e)
    {
        e.ToTable("service_orders");
        e.Property(p => p.Id).HasColumnName("id");
        e.Property(p => p.UserId).HasColumnName("user_id");
        e.Property(p => p.ServiceId).HasColumnName("service_id");
        e.Property(p => p.Status).HasColumnName("status");
        e.Property(p => p.ScheduledDate).HasColumnName("scheduled_date");
        e.Property(p => p.TotalPrice).HasColumnName("total_price");
        e.Property(p => p.Notes).HasColumnName("notes");
        e.Property(p => p.Address).HasColumnName("address");
        e.Property(p => p.CreatedAt).HasColumnName("created_at");

        e.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);

        e.HasOne(p => p.Service)
            .WithMany()
            .HasForeignKey(p => p.ServiceId);
    }

    private void ServiceFeedbackMap(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ServiceFeedback> e)
    {
        e.ToTable("service_feedbacks");
        e.Property(p => p.Id).HasColumnName("id");
        e.Property(p => p.ServiceOrderId).HasColumnName("service_order_id");
        e.Property(p => p.UserId).HasColumnName("user_id");
        e.Property(p => p.Rating).HasColumnName("rating");
        e.Property(p => p.Comments).HasColumnName("comments");

        e.HasOne(p => p.ServiceOrder)
            .WithMany()
            .HasForeignKey(p => p.ServiceOrderId);

        e.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);
    }
}

class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
}

class ProductPrice
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public decimal Price { get; set; }
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string? SupplierLink { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Brand { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? SpecsJson { get; set; }
    public int? TdpWatt { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Socket { get; set; }
    public string? Capacity { get; set; }
    public string? Type { get; set; }
    public string? ImageUrl1 { get; set; }
    public string? ImageUrl2 { get; set; }
    public string? ImageUrl3 { get; set; }
    public string? ImageUrl4 { get; set; }
    public string? ImageUrl5 { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<ProductPrice> ProductPrices { get; set; } = new();
}

static class ProductExtensions
{
    // Không deserialize nữa để tránh lỗi khi cột specs chứa plain text; chỉ trả raw string
    private static string? SpecsRaw(string? json) => string.IsNullOrWhiteSpace(json) ? null : json;

    public static object ToDto(this Product p) => new
    {
        id = p.Id,
        name = p.Name,
        brand = p.Brand,
        category_id = p.CategoryId,
        category = p.Category is null ? null : new { id = p.Category.Id, name = p.Category.Name },
        image_url1 = p.ImageUrl1,
        imageUrl1 = p.ImageUrl1,
        productPrices = (p.ProductPrices ?? new List<ProductPrice>()).Select(pp => new { price = pp.Price }),
        specs = SpecsRaw(p.SpecsJson),
        socket = p.Socket,
        tdpWatt = p.TdpWatt,
        tdp_watt = p.TdpWatt
    };
}

class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public List<ProductPrice> ProductPrices { get; set; } = new();
}

class ServiceEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? BasePrice { get; set; }
    public string? Unit { get; set; }
}

class ServiceOrder
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public int? ServiceId { get; set; }
    public ServiceEntity? Service { get; set; }
    public string? Status { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Notes { get; set; }
    public string? Address { get; set; }
    public DateTime? CreatedAt { get; set; }
}

class ServiceFeedback
{
    public int Id { get; set; }
    public int? ServiceOrderId { get; set; }
    public ServiceOrder? ServiceOrder { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public int? Rating { get; set; }
    public string? Comments { get; set; }
}

class User
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string Fullname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Address { get; set; }
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; }
}

class RegisterRequest
{
    public string? fullname { get; set; }
    public string? email { get; set; }
    public string? password { get; set; }
    public string? phone { get; set; }
    public string? dob { get; set; }
    public string? address { get; set; }
}

class LoginRequest
{
    public string? identifier { get; set; }
    public string? password { get; set; }
}

class ProductPriceDto
{
    public decimal price { get; set; }
}

class ProductCreateUpdateDto
{
    public string? name { get; set; }
    public string? brand { get; set; }
    public int category_id { get; set; }
    public string? image_url1 { get; set; }
    public string? imageUrl1 { get; set; }
    public List<ProductPriceDto>? productPrices { get; set; }
    public Dictionary<string, object>? specs { get; set; }
    public string? socket { get; set; }
    public int? tdpWatt { get; set; }
    public int? tdp_watt { get; set; }
}
