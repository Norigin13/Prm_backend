using Microsoft.EntityFrameworkCore;
using MyBackend.Data;

namespace MyBackend.Services;

public class SupplierService
{
    private readonly AppDbContext _dbContext;

    public SupplierService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> GetAllSuppliersAsync()
    {
        var suppliers = await _dbContext.Suppliers.ToListAsync();
        return Results.Ok(suppliers);
    }

    public async Task<IResult> GetSupplierByIdAsync(int id)
    {
        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
        return supplier is null ? Results.NotFound(new { message = "Supplier not found" }) : Results.Ok(supplier);
    }

    public async Task<IResult> CreateSupplierAsync(SupplierCreateUpdateDto dto)
    {
        var supplier = new Supplier
        {
            Name = dto.name ?? "Unnamed Supplier",
            Website = dto.website
        };

        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(supplier);
    }

    public async Task<IResult> UpdateSupplierAsync(int id, SupplierCreateUpdateDto dto)
    {
        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
        if (supplier is null) return Results.NotFound(new { message = "Supplier not found" });

        if (dto.name is not null) supplier.Name = dto.name;
        if (dto.website is not null) supplier.Website = dto.website;

        await _dbContext.SaveChangesAsync();
        return Results.Ok(supplier);
    }

    public async Task<IResult> DeleteSupplierAsync(int id)
    {
        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
        if (supplier is null) return Results.NotFound(new { message = "Supplier not found" });

        _dbContext.Suppliers.Remove(supplier);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(new { success = true });
    }

    public async Task<IResult> GetSupplierWithProductsAsync(int id)
    {
        var supplier = await _dbContext.Suppliers
            .Include(s => s.ProductPrices)
            .ThenInclude(pp => pp.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        return supplier is null ? Results.NotFound(new { message = "Supplier not found" }) : Results.Ok(new
        {
            id = supplier.Id,
            name = supplier.Name,
            website = supplier.Website,
            products = supplier.ProductPrices.Select(pp => new
            {
                productId = pp.Product.Id,
                productName = pp.Product.Name,
                price = pp.Price,
                supplierLink = pp.SupplierLink
            })
        });
    }
}

// Supplier models
public class SupplierCreateUpdateDto
{
    public string? name { get; set; }
    public string? website { get; set; }
}
