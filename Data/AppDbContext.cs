using Microsoft.EntityFrameworkCore;

namespace PRM_Backend.Data;

public class AppDbContext : DbContext
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
    public DbSet<Build> Builds => Set<Build>();
    public DbSet<BuildItem> BuildItems => Set<BuildItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

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
        modelBuilder.Entity<Build>(BuildMap);
        modelBuilder.Entity<BuildItem>(BuildItemMap);
        modelBuilder.Entity<Order>(OrderMap);
        modelBuilder.Entity<ChatMessage>(ChatMessageMap);
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

    private void BuildMap(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Build> e)
    {
        e.ToTable("builds");
        e.Property(p => p.Id).HasColumnName("id");
        e.Property(p => p.UserId).HasColumnName("user_id");
        e.Property(p => p.Name).HasColumnName("name");
        e.Property(p => p.TotalPrice).HasColumnName("total_price");
        e.Property(p => p.CreatedAt).HasColumnName("created_at");

        e.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);
    }

    private void BuildItemMap(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<BuildItem> e)
    {
        e.ToTable("build_items");
        e.Property(p => p.Id).HasColumnName("id");
        e.Property(p => p.BuildId).HasColumnName("build_id");
        e.Property(p => p.ProductPriceId).HasColumnName("product_price_id");
        e.Property(p => p.Quantity).HasColumnName("quantity");

        e.HasOne(p => p.Build)
            .WithMany(b => b.Items)
            .HasForeignKey(p => p.BuildId);

        e.HasOne(p => p.ProductPrice)
            .WithMany()
            .HasForeignKey(p => p.ProductPriceId);
    }

    private void OrderMap(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Order> e)
    {
        e.ToTable("orders");
        e.Property(p => p.Id).HasColumnName("id");
        e.Property(p => p.UserId).HasColumnName("user_id");
        e.Property(p => p.BuildId).HasColumnName("build_id");
        e.Property(p => p.Status).HasColumnName("status");
        e.Property(p => p.TotalPrice).HasColumnName("total_price");
        e.Property(p => p.PaymentMethod).HasColumnName("payment_method");
        e.Property(p => p.Phone).HasColumnName("phone");
        e.Property(p => p.Address).HasColumnName("address");
        e.Property(p => p.CreatedAt).HasColumnName("created_at");

        e.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);

        e.HasOne(p => p.Build)
            .WithMany()
            .HasForeignKey(p => p.BuildId);
    }

    private void ChatMessageMap(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ChatMessage> e)
    {
        e.ToTable("chat_messages");
        e.Property(p => p.Id).HasColumnName("id");
        e.Property(p => p.UserId).HasColumnName("user_id");
        e.Property(p => p.Message).HasColumnName("message");
        e.Property(p => p.Response).HasColumnName("response");
        e.Property(p => p.CreatedAt).HasColumnName("created_at");

        e.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);
    }
}

// Models
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
}

public class ProductPrice
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

public class Product
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

public static class ProductExtensions
{
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

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public List<ProductPrice> ProductPrices { get; set; } = new();
}

public class ServiceEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? BasePrice { get; set; }
    public string? Unit { get; set; }
}

public class ServiceOrder
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

public class ServiceFeedback
{
    public int Id { get; set; }
    public int? ServiceOrderId { get; set; }
    public ServiceOrder? ServiceOrder { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public int? Rating { get; set; }
    public string? Comments { get; set; }
}

public class User
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

public class Build
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? TotalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<BuildItem> Items { get; set; } = new();
}

public class BuildItem
{
    public int Id { get; set; }
    public int BuildId { get; set; }
    public Build Build { get; set; } = null!;
    public int ProductPriceId { get; set; }
    public ProductPrice ProductPrice { get; set; } = null!;
    public int Quantity { get; set; } = 1;
}

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public int? BuildId { get; set; }
    public Build? Build { get; set; }
    public string? Status { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? PaymentMethod { get; set; }
    public long? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ChatMessage
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Response { get; set; }
    public DateTime? CreatedAt { get; set; }
}
