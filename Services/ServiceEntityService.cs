using Microsoft.EntityFrameworkCore;
using MyBackend.Data;

namespace MyBackend.Services;

public class ServiceEntityService
{
    private readonly AppDbContext _dbContext;

    public ServiceEntityService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> GetAllServicesAsync()
    {
        var services = await _dbContext.Services.ToListAsync();
        return Results.Ok(services);
    }

    public async Task<IResult> GetServiceByIdAsync(int id)
    {
        var service = await _dbContext.Services.FirstOrDefaultAsync(s => s.Id == id);
        return service is null ? Results.NotFound(new { message = "Service not found" }) : Results.Ok(service);
    }

    public async Task<IResult> CreateServiceAsync(ServiceEntityCreateUpdateDto dto)
    {
        var service = new ServiceEntity
        {
            Name = dto.name ?? "Unnamed Service",
            Description = dto.description,
            BasePrice = dto.basePrice,
            Unit = dto.unit
        };

        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(service);
    }

    public async Task<IResult> UpdateServiceAsync(int id, ServiceEntityCreateUpdateDto dto)
    {
        var service = await _dbContext.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service is null) return Results.NotFound(new { message = "Service not found" });

        if (dto.name is not null) service.Name = dto.name;
        if (dto.description is not null) service.Description = dto.description;
        if (dto.basePrice.HasValue) service.BasePrice = dto.basePrice;
        if (dto.unit is not null) service.Unit = dto.unit;

        await _dbContext.SaveChangesAsync();
        return Results.Ok(service);
    }

    public async Task<IResult> DeleteServiceAsync(int id)
    {
        var service = await _dbContext.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service is null) return Results.NotFound(new { message = "Service not found" });

        _dbContext.Services.Remove(service);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(new { success = true });
    }
}

// Service Entity models
public class ServiceEntityCreateUpdateDto
{
    public string? name { get; set; }
    public string? description { get; set; }
    public int? basePrice { get; set; }
    public string? unit { get; set; }
}
