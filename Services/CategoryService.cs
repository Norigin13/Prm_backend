using Microsoft.EntityFrameworkCore;
using MyBackend.Data;

namespace MyBackend.Services;

public class CategoryService
{
    private readonly AppDbContext _dbContext;

    public CategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> GetAllCategoriesAsync()
    {
        var categories = await _dbContext.Categories.ToListAsync();
        return Results.Ok(categories);
    }

    public async Task<IResult> GetCategoryByIdAsync(int id)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        return category is null ? Results.NotFound(new { message = "Category not found" }) : Results.Ok(category);
    }

    public async Task<IResult> CreateCategoryAsync(CategoryCreateUpdateDto dto)
    {
        var category = new Category
        {
            Name = dto.name ?? "Unnamed Category"
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(category);
    }

    public async Task<IResult> UpdateCategoryAsync(int id, CategoryCreateUpdateDto dto)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return Results.NotFound(new { message = "Category not found" });

        if (dto.name is not null) category.Name = dto.name;

        await _dbContext.SaveChangesAsync();
        return Results.Ok(category);
    }

    public async Task<IResult> DeleteCategoryAsync(int id)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return Results.NotFound(new { message = "Category not found" });

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(new { success = true });
    }

    public async Task<IResult> GetCategoryWithProductsAsync(int id)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Products)
            .ThenInclude(p => p.ProductPrices)
            .FirstOrDefaultAsync(c => c.Id == id);

        return category is null ? Results.NotFound(new { message = "Category not found" }) : Results.Ok(new
        {
            id = category.Id,
            name = category.Name,
            products = category.Products.Select(p => p.ToDto())
        });
    }
}

// Category models
public class CategoryCreateUpdateDto
{
    public string? name { get; set; }
}
