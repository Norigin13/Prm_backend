using Microsoft.EntityFrameworkCore;
using PRM_Backend.Data;
using PRM_Backend.Services;

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
                "http://localhost:5162",
                "https://your-frontend-domain.com",
                "https://your-vercel-app.vercel.app"
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

// Register Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<ServiceEntityService>();
builder.Services.AddScoped<ServiceOrderService>();
builder.Services.AddScoped<ServiceFeedbackService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<BuildService>();
builder.Services.AddScoped<OrderService>();

// Register HttpClient for AI API
builder.Services.AddHttpClient<ChatService>();

var app = builder.Build();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRM_Backend API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
});

// Không bắt buộc redirect HTTPS khi dev
// app.UseHttpsRedirection();
app.UseCors("AllowDev");

// Migrate/EnsureCreated + seed tối thiểu
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    
    // Seed Categories
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
    
    // Seed Demo User
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

// ==================== USER ENDPOINTS ====================
app.MapPost("/api/user/register", async (RegisterRequest req, UserService userService) => 
    await userService.RegisterAsync(req))
    .WithTags("User");

app.MapPost("/api/user/login", async (LoginRequest req, UserService userService) => 
    await userService.LoginAsync(req))
    .WithTags("User");

app.MapGet("/api/user/{userId:int}", async (int userId, UserService userService) => 
    await userService.GetUserAsync(userId))
    .WithTags("User");

app.MapPut("/api/user/{userId:int}", async (int userId, Dictionary<string, object> update, UserService userService) => 
    await userService.UpdateUserAsync(userId, update))
    .WithTags("User");

app.MapDelete("/api/user/{userId:int}", async (int userId, UserService userService) => 
    await userService.DeleteUserAsync(userId))
    .WithTags("User");

app.MapGet("/api/users", async (UserService userService) => 
    await userService.GetAllUsersAsync())
    .WithTags("User");

// ==================== CATEGORY ENDPOINTS ====================
app.MapGet("/api/category", async (CategoryService categoryService) => 
    await categoryService.GetAllCategoriesAsync())
    .WithTags("Category");

app.MapGet("/api/category/{id:int}", async (int id, CategoryService categoryService) => 
    await categoryService.GetCategoryByIdAsync(id))
    .WithTags("Category");

app.MapPost("/api/category", async (CategoryCreateUpdateDto dto, CategoryService categoryService) => 
    await categoryService.CreateCategoryAsync(dto))
    .WithTags("Category");

app.MapPut("/api/category/{id:int}", async (int id, CategoryCreateUpdateDto dto, CategoryService categoryService) => 
    await categoryService.UpdateCategoryAsync(id, dto))
    .WithTags("Category");

app.MapDelete("/api/category/{id:int}", async (int id, CategoryService categoryService) => 
    await categoryService.DeleteCategoryAsync(id))
    .WithTags("Category");

app.MapGet("/api/category/{id:int}/products", async (int id, CategoryService categoryService) => 
    await categoryService.GetCategoryWithProductsAsync(id))
    .WithTags("Category");

// ==================== PRODUCT ENDPOINTS ====================
app.MapGet("/api/product", async (ProductService productService) => 
    await productService.GetAllProductsAsync())
    .WithTags("Product");

app.MapGet("/api/product/{id:int}", async (int id, ProductService productService) => 
    await productService.GetProductByIdAsync(id))
    .WithTags("Product");

app.MapPost("/api/product", async (ProductCreateUpdateDto dto, ProductService productService) => 
    await productService.CreateProductAsync(dto))
    .WithTags("Product");

app.MapPut("/api/product/{id:int}", async (int id, ProductCreateUpdateDto dto, ProductService productService) => 
    await productService.UpdateProductAsync(id, dto))
    .WithTags("Product");

app.MapDelete("/api/product/{id:int}", async (int id, ProductService productService) => 
    await productService.DeleteProductAsync(id))
    .WithTags("Product");

// ==================== SUPPLIER ENDPOINTS ====================
app.MapGet("/api/supplier", async (SupplierService supplierService) => 
    await supplierService.GetAllSuppliersAsync())
    .WithTags("Supplier");

app.MapGet("/api/supplier/{id:int}", async (int id, SupplierService supplierService) => 
    await supplierService.GetSupplierByIdAsync(id))
    .WithTags("Supplier");

app.MapPost("/api/supplier", async (SupplierCreateUpdateDto dto, SupplierService supplierService) => 
    await supplierService.CreateSupplierAsync(dto))
    .WithTags("Supplier");

app.MapPut("/api/supplier/{id:int}", async (int id, SupplierCreateUpdateDto dto, SupplierService supplierService) => 
    await supplierService.UpdateSupplierAsync(id, dto))
    .WithTags("Supplier");

app.MapDelete("/api/supplier/{id:int}", async (int id, SupplierService supplierService) => 
    await supplierService.DeleteSupplierAsync(id))
    .WithTags("Supplier");

app.MapGet("/api/supplier/{id:int}/products", async (int id, SupplierService supplierService) => 
    await supplierService.GetSupplierWithProductsAsync(id))
    .WithTags("Supplier");

// ==================== SERVICE ENTITY ENDPOINTS ====================
app.MapGet("/api/service", async (ServiceEntityService serviceEntityService) => 
    await serviceEntityService.GetAllServicesAsync())
    .WithTags("Service");

app.MapGet("/api/service/{id:int}", async (int id, ServiceEntityService serviceEntityService) => 
    await serviceEntityService.GetServiceByIdAsync(id))
    .WithTags("Service");

app.MapPost("/api/service", async (ServiceEntityCreateUpdateDto dto, ServiceEntityService serviceEntityService) => 
    await serviceEntityService.CreateServiceAsync(dto))
    .WithTags("Service");

app.MapPut("/api/service/{id:int}", async (int id, ServiceEntityCreateUpdateDto dto, ServiceEntityService serviceEntityService) => 
    await serviceEntityService.UpdateServiceAsync(id, dto))
    .WithTags("Service");

app.MapDelete("/api/service/{id:int}", async (int id, ServiceEntityService serviceEntityService) => 
    await serviceEntityService.DeleteServiceAsync(id))
    .WithTags("Service");

// ==================== SERVICE ORDER ENDPOINTS ====================
app.MapGet("/api/service-order", async (ServiceOrderService serviceOrderService) => 
    await serviceOrderService.GetAllServiceOrdersAsync())
    .WithTags("Service Order");

app.MapGet("/api/service-order/{id:int}", async (int id, ServiceOrderService serviceOrderService) => 
    await serviceOrderService.GetServiceOrderByIdAsync(id))
    .WithTags("Service Order");

app.MapPost("/api/service-order", async (ServiceOrderCreateUpdateDto dto, ServiceOrderService serviceOrderService) => 
    await serviceOrderService.CreateServiceOrderAsync(dto))
    .WithTags("Service Order");

app.MapPut("/api/service-order/{id:int}", async (int id, ServiceOrderCreateUpdateDto dto, ServiceOrderService serviceOrderService) => 
    await serviceOrderService.UpdateServiceOrderAsync(id, dto))
    .WithTags("Service Order");

app.MapDelete("/api/service-order/{id:int}", async (int id, ServiceOrderService serviceOrderService) => 
    await serviceOrderService.DeleteServiceOrderAsync(id))
    .WithTags("Service Order");

app.MapGet("/api/service-order/user/{userId:int}", async (int userId, ServiceOrderService serviceOrderService) => 
    await serviceOrderService.GetServiceOrdersByUserAsync(userId))
    .WithTags("Service Order");

// ==================== SERVICE FEEDBACK ENDPOINTS ====================
app.MapGet("/api/service-feedback", async (ServiceFeedbackService serviceFeedbackService) => 
    await serviceFeedbackService.GetAllServiceFeedbacksAsync())
    .WithTags("Service Feedback");

app.MapGet("/api/service-feedback/{id:int}", async (int id, ServiceFeedbackService serviceFeedbackService) => 
    await serviceFeedbackService.GetServiceFeedbackByIdAsync(id))
    .WithTags("Service Feedback");

app.MapPost("/api/service-feedback", async (ServiceFeedbackCreateUpdateDto dto, ServiceFeedbackService serviceFeedbackService) => 
    await serviceFeedbackService.CreateServiceFeedbackAsync(dto))
    .WithTags("Service Feedback");

app.MapPut("/api/service-feedback/{id:int}", async (int id, ServiceFeedbackCreateUpdateDto dto, ServiceFeedbackService serviceFeedbackService) => 
    await serviceFeedbackService.UpdateServiceFeedbackAsync(id, dto))
    .WithTags("Service Feedback");

app.MapDelete("/api/service-feedback/{id:int}", async (int id, ServiceFeedbackService serviceFeedbackService) => 
    await serviceFeedbackService.DeleteServiceFeedbackAsync(id))
    .WithTags("Service Feedback");

app.MapGet("/api/service-feedback/service-order/{serviceOrderId:int}", async (int serviceOrderId, ServiceFeedbackService serviceFeedbackService) => 
    await serviceFeedbackService.GetServiceFeedbacksByServiceOrderAsync(serviceOrderId))
    .WithTags("Service Feedback");

app.MapGet("/api/service-feedback/user/{userId:int}", async (int userId, ServiceFeedbackService serviceFeedbackService) => 
    await serviceFeedbackService.GetServiceFeedbacksByUserAsync(userId))
    .WithTags("Service Feedback");

// ==================== CHAT AI ENDPOINTS ====================
app.MapPost("/api/chat/send", async (ChatRequest request, ChatService chatService) => 
    await chatService.ChatAsync(request))
    .WithTags("Chat AI");

app.MapPost("/api/chat/simple", async (SimpleChatRequest request, ChatService chatService) => 
{
    var chatRequest = new ChatRequest
    {
        Message = request.Message,
        UserId = "simple_user",
        ChatHistory = null
    };
    return await chatService.ChatAsync(chatRequest);
})
    .WithTags("Chat AI");

app.MapGet("/api/chat/history", async (int? userId, ChatService chatService) => 
    await chatService.GetChatHistoryAsync(userId))
    .WithTags("Chat AI");

// ==================== BUILD ENDPOINTS ====================
app.MapGet("/api/build", async (BuildService buildService) => 
    await buildService.GetAllBuildsAsync())
    .WithTags("Build");

app.MapGet("/api/build/{id:int}", async (int id, BuildService buildService) => 
    await buildService.GetBuildByIdAsync(id))
    .WithTags("Build");

app.MapPost("/api/build", async (BuildCreateRequest request, BuildService buildService) => 
    await buildService.CreateBuildAsync(request))
    .WithTags("Build");

app.MapPut("/api/build/{id:int}", async (int id, Build updateBuild, BuildService buildService) => 
    await buildService.UpdateBuildAsync(id, updateBuild))
    .WithTags("Build");

app.MapDelete("/api/build/{id:int}", async (int id, BuildService buildService) => 
    await buildService.DeleteBuildAsync(id))
    .WithTags("Build");

app.MapGet("/api/build/user/{userId:int}", async (int userId, BuildService buildService) => 
    await buildService.GetBuildsByUserAsync(userId))
    .WithTags("Build");

// ==================== ORDER ENDPOINTS ====================
app.MapGet("/api/order", async (int? userId, OrderService orderService) => 
{
    if (userId.HasValue)
        return await orderService.GetOrdersByUserAsync(userId.Value);
    return await orderService.GetAllOrdersAsync();
})
    .WithTags("Order");

app.MapGet("/api/order/{id:int}", async (int id, OrderService orderService) => 
    await orderService.GetOrderByIdAsync(id))
    .WithTags("Order");

app.MapGet("/api/order/user/{userId:int}", async (int userId, OrderService orderService) => 
    await orderService.GetOrdersByUserAsync(userId))
    .WithTags("Order");

app.MapPost("/api/order", async (OrderCreateRequest request, OrderService orderService) => 
    await orderService.CreateOrderAsync(request))
    .WithTags("Order");

app.MapPut("/api/order/{id:int}", async (int id, Order updateOrder, OrderService orderService) => 
    await orderService.UpdateOrderAsync(id, updateOrder))
    .WithTags("Order");

app.MapDelete("/api/order/{id:int}", async (int id, OrderService orderService) => 
    await orderService.DeleteOrderAsync(id))
    .WithTags("Order");

// ==================== DEMO ENDPOINT ====================
// Demo weatherforecast giữ lại nếu cần
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithTags("Demo");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}