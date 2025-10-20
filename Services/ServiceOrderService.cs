using Microsoft.EntityFrameworkCore;
using PRM_Backend.Data;

namespace PRM_Backend.Services;

public class ServiceOrderService
{
    private readonly AppDbContext _dbContext;

    public ServiceOrderService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> GetAllServiceOrdersAsync()
    {
        var serviceOrders = await _dbContext.ServiceOrders
            .Include(so => so.User)
            .Include(so => so.Service)
            .ToListAsync();

        return Results.Ok(serviceOrders.Select(so => new
        {
            id = so.Id,
            userId = so.UserId,
            user = so.User != null ? new { id = so.User.Id, fullname = so.User.Fullname } : null,
            serviceId = so.ServiceId,
            service = so.Service != null ? new { id = so.Service.Id, name = so.Service.Name } : null,
            status = so.Status,
            scheduledDate = so.ScheduledDate,
            totalPrice = so.TotalPrice,
            notes = so.Notes,
            address = so.Address,
            createdAt = so.CreatedAt
        }));
    }

    public async Task<IResult> GetServiceOrderByIdAsync(int id)
    {
        var serviceOrder = await _dbContext.ServiceOrders
            .Include(so => so.User)
            .Include(so => so.Service)
            .FirstOrDefaultAsync(so => so.Id == id);

        return serviceOrder is null ? Results.NotFound(new { message = "Service order not found" }) : Results.Ok(new
        {
            id = serviceOrder.Id,
            userId = serviceOrder.UserId,
            user = serviceOrder.User != null ? new { id = serviceOrder.User.Id, fullname = serviceOrder.User.Fullname } : null,
            serviceId = serviceOrder.ServiceId,
            service = serviceOrder.Service != null ? new { id = serviceOrder.Service.Id, name = serviceOrder.Service.Name } : null,
            status = serviceOrder.Status,
            scheduledDate = serviceOrder.ScheduledDate,
            totalPrice = serviceOrder.TotalPrice,
            notes = serviceOrder.Notes,
            address = serviceOrder.Address,
            createdAt = serviceOrder.CreatedAt
        });
    }

    public async Task<IResult> CreateServiceOrderAsync(ServiceOrderCreateUpdateDto dto)
    {
        var serviceOrder = new ServiceOrder
        {
            UserId = dto.userId,
            ServiceId = dto.serviceId,
            Status = dto.status ?? "Pending",
            ScheduledDate = dto.scheduledDate,
            TotalPrice = dto.totalPrice,
            Notes = dto.notes,
            Address = dto.address,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ServiceOrders.Add(serviceOrder);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(serviceOrder);
    }

    public async Task<IResult> UpdateServiceOrderAsync(int id, ServiceOrderCreateUpdateDto dto)
    {
        var serviceOrder = await _dbContext.ServiceOrders.FirstOrDefaultAsync(so => so.Id == id);
        if (serviceOrder is null) return Results.NotFound(new { message = "Service order not found" });

        if (dto.userId.HasValue) serviceOrder.UserId = dto.userId;
        if (dto.serviceId.HasValue) serviceOrder.ServiceId = dto.serviceId;
        if (dto.status is not null) serviceOrder.Status = dto.status;
        if (dto.scheduledDate.HasValue) serviceOrder.ScheduledDate = dto.scheduledDate;
        if (dto.totalPrice.HasValue) serviceOrder.TotalPrice = dto.totalPrice;
        if (dto.notes is not null) serviceOrder.Notes = dto.notes;
        if (dto.address is not null) serviceOrder.Address = dto.address;

        await _dbContext.SaveChangesAsync();
        return Results.Ok(serviceOrder);
    }

    public async Task<IResult> DeleteServiceOrderAsync(int id)
    {
        var serviceOrder = await _dbContext.ServiceOrders.FirstOrDefaultAsync(so => so.Id == id);
        if (serviceOrder is null) return Results.NotFound(new { message = "Service order not found" });

        _dbContext.ServiceOrders.Remove(serviceOrder);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(new { success = true });
    }

    public async Task<IResult> GetServiceOrdersByUserAsync(int userId)
    {
        var serviceOrders = await _dbContext.ServiceOrders
            .Include(so => so.Service)
            .Where(so => so.UserId == userId)
            .ToListAsync();

        return Results.Ok(serviceOrders.Select(so => new
        {
            id = so.Id,
            serviceId = so.ServiceId,
            service = so.Service != null ? new { id = so.Service.Id, name = so.Service.Name } : null,
            status = so.Status,
            scheduledDate = so.ScheduledDate,
            totalPrice = so.TotalPrice,
            notes = so.Notes,
            address = so.Address,
            createdAt = so.CreatedAt
        }));
    }
}

// Service Order models
public class ServiceOrderCreateUpdateDto
{
    public int? userId { get; set; }
    public int? serviceId { get; set; }
    public string? status { get; set; }
    public DateTime? scheduledDate { get; set; }
    public decimal? totalPrice { get; set; }
    public string? notes { get; set; }
    public string? address { get; set; }
}
