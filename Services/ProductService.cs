using Microsoft.EntityFrameworkCore;
using MyBackend.Data;
using System.Text.Json;

namespace MyBackend.Services;

public class ProductService
{
    private readonly AppDbContext _dbContext;

    public ProductService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> GetAllProductsAsync()
    {
        var list = await _dbContext.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductPrices)
            .ToListAsync();

        return Results.Ok(list.Select(p => p.ToDto()));
    }

    public async Task<IResult> GetProductByIdAsync(int id)
    {
        var found = await _dbContext.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductPrices)
            .FirstOrDefaultAsync(p => p.Id == id);

        return found is null ? Results.NotFound(new { message = "Product not found" }) : Results.Ok(found.ToDto());
    }

    public async Task<IResult> CreateProductAsync(ProductCreateUpdateDto dto)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == dto.category_id) 
                      ?? await _dbContext.Categories.FirstAsync();

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

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(product.ToDto());
    }

    public async Task<IResult> UpdateProductAsync(int id, ProductCreateUpdateDto dto)
    {
        var found = await _dbContext.Products
            .Include(p => p.ProductPrices)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (found is null) return Results.NotFound(new { message = "Product not found" });

        if (dto.name is not null) found.Name = dto.name;
        if (dto.brand is not null) found.Brand = dto.brand;
        if (dto.image_url1 is not null || dto.imageUrl1 is not null) found.ImageUrl1 = dto.image_url1 ?? dto.imageUrl1!;
        if (dto.socket is not null) found.Socket = dto.socket;
        if (dto.tdpWatt.HasValue || dto.tdp_watt.HasValue) found.TdpWatt = dto.tdpWatt ?? dto.tdp_watt;
        
        if (dto.category_id > 0)
        {
            var cat = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == dto.category_id);
            if (cat is not null) { found.CategoryId = cat.Id; found.Category = cat; }
        }
        
        if (dto.productPrices is not null)
        {
            found.ProductPrices = dto.productPrices.Select(pp => new ProductPrice { Price = pp.price }).ToList();
        }
        
        if (dto.specs is not null) found.SpecsJson = JsonSerializer.Serialize(dto.specs);

        await _dbContext.SaveChangesAsync();
        return Results.Ok(found.ToDto());
    }

    public async Task<IResult> DeleteProductAsync(int id)
    {
        var found = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (found is null) return Results.NotFound(new { message = "Product not found" });

        _dbContext.Products.Remove(found);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(new { success = true });
    }
}

// Product models
public class ProductPriceDto
{
    public decimal price { get; set; }
}

public class ProductCreateUpdateDto
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
