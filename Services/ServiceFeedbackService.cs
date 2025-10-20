using Microsoft.EntityFrameworkCore;
using PRM_Backend.Data;

namespace PRM_Backend.Services;

public class ServiceFeedbackService
{
    private readonly AppDbContext _dbContext;

    public ServiceFeedbackService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> GetAllServiceFeedbacksAsync()
    {
        var serviceFeedbacks = await _dbContext.ServiceFeedbacks
            .Include(sf => sf.ServiceOrder)
            .Include(sf => sf.User)
            .ToListAsync();

        return Results.Ok(serviceFeedbacks.Select(sf => new
        {
            id = sf.Id,
            serviceOrderId = sf.ServiceOrderId,
            serviceOrder = sf.ServiceOrder != null ? new { id = sf.ServiceOrder.Id, status = sf.ServiceOrder.Status } : null,
            userId = sf.UserId,
            user = sf.User != null ? new { id = sf.User.Id, fullname = sf.User.Fullname } : null,
            rating = sf.Rating,
            comments = sf.Comments
        }));
    }

    public async Task<IResult> GetServiceFeedbackByIdAsync(int id)
    {
        var serviceFeedback = await _dbContext.ServiceFeedbacks
            .Include(sf => sf.ServiceOrder)
            .Include(sf => sf.User)
            .FirstOrDefaultAsync(sf => sf.Id == id);

        return serviceFeedback is null ? Results.NotFound(new { message = "Service feedback not found" }) : Results.Ok(new
        {
            id = serviceFeedback.Id,
            serviceOrderId = serviceFeedback.ServiceOrderId,
            serviceOrder = serviceFeedback.ServiceOrder != null ? new { id = serviceFeedback.ServiceOrder.Id, status = serviceFeedback.ServiceOrder.Status } : null,
            userId = serviceFeedback.UserId,
            user = serviceFeedback.User != null ? new { id = serviceFeedback.User.Id, fullname = serviceFeedback.User.Fullname } : null,
            rating = serviceFeedback.Rating,
            comments = serviceFeedback.Comments
        });
    }

    public async Task<IResult> CreateServiceFeedbackAsync(ServiceFeedbackCreateUpdateDto dto)
    {
        var serviceFeedback = new ServiceFeedback
        {
            ServiceOrderId = dto.serviceOrderId,
            UserId = dto.userId,
            Rating = dto.rating,
            Comments = dto.comments
        };

        _dbContext.ServiceFeedbacks.Add(serviceFeedback);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(serviceFeedback);
    }

    public async Task<IResult> UpdateServiceFeedbackAsync(int id, ServiceFeedbackCreateUpdateDto dto)
    {
        var serviceFeedback = await _dbContext.ServiceFeedbacks.FirstOrDefaultAsync(sf => sf.Id == id);
        if (serviceFeedback is null) return Results.NotFound(new { message = "Service feedback not found" });

        if (dto.serviceOrderId.HasValue) serviceFeedback.ServiceOrderId = dto.serviceOrderId;
        if (dto.userId.HasValue) serviceFeedback.UserId = dto.userId;
        if (dto.rating.HasValue) serviceFeedback.Rating = dto.rating;
        if (dto.comments is not null) serviceFeedback.Comments = dto.comments;

        await _dbContext.SaveChangesAsync();
        return Results.Ok(serviceFeedback);
    }

    public async Task<IResult> DeleteServiceFeedbackAsync(int id)
    {
        var serviceFeedback = await _dbContext.ServiceFeedbacks.FirstOrDefaultAsync(sf => sf.Id == id);
        if (serviceFeedback is null) return Results.NotFound(new { message = "Service feedback not found" });

        _dbContext.ServiceFeedbacks.Remove(serviceFeedback);
        await _dbContext.SaveChangesAsync();
        return Results.Ok(new { success = true });
    }

    public async Task<IResult> GetServiceFeedbacksByServiceOrderAsync(int serviceOrderId)
    {
        var serviceFeedbacks = await _dbContext.ServiceFeedbacks
            .Include(sf => sf.User)
            .Where(sf => sf.ServiceOrderId == serviceOrderId)
            .ToListAsync();

        return Results.Ok(serviceFeedbacks.Select(sf => new
        {
            id = sf.Id,
            userId = sf.UserId,
            user = sf.User != null ? new { id = sf.User.Id, fullname = sf.User.Fullname } : null,
            rating = sf.Rating,
            comments = sf.Comments
        }));
    }

    public async Task<IResult> GetServiceFeedbacksByUserAsync(int userId)
    {
        var serviceFeedbacks = await _dbContext.ServiceFeedbacks
            .Include(sf => sf.ServiceOrder)
            .Where(sf => sf.UserId == userId)
            .ToListAsync();

        return Results.Ok(serviceFeedbacks.Select(sf => new
        {
            id = sf.Id,
            serviceOrderId = sf.ServiceOrderId,
            serviceOrder = sf.ServiceOrder != null ? new { id = sf.ServiceOrder.Id, status = sf.ServiceOrder.Status } : null,
            rating = sf.Rating,
            comments = sf.Comments
        }));
    }
}

// Service Feedback models
public class ServiceFeedbackCreateUpdateDto
{
    public int? serviceOrderId { get; set; }
    public int? userId { get; set; }
    public int? rating { get; set; }
    public string? comments { get; set; }
}
