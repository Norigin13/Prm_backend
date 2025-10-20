using PRM_Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace PRM_Backend.Services;

public class OrderService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Build)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Build)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetOrdersByUserAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Build)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task<Order> CreateOrderAsync(OrderCreateRequest request)
    {
        var order = new Order
        {
            UserId = request.UserId,
            BuildId = request.BuildId,
            TotalPrice = request.TotalPrice,
            Status = request.Status ?? "PAID",
            PaymentMethod = request.PaymentMethod,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow
        };

        // Xử lý phone number
        if (!string.IsNullOrEmpty(request.Phone))
        {
            var phoneDigits = new string(request.Phone.Where(char.IsDigit).ToArray());
            if (long.TryParse(phoneDigits, out var phoneNumber))
            {
                order.Phone = phoneNumber;
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(order.Id) ?? order;
    }

    public async Task<Order?> UpdateOrderAsync(int id, Order updateOrder)
    {
        var existingOrder = await _context.Orders.FindAsync(id);
        if (existingOrder == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(updateOrder.Status))
            existingOrder.Status = updateOrder.Status;

        if (updateOrder.TotalPrice.HasValue)
            existingOrder.TotalPrice = updateOrder.TotalPrice;

        if (!string.IsNullOrEmpty(updateOrder.PaymentMethod))
            existingOrder.PaymentMethod = updateOrder.PaymentMethod;

        if (!string.IsNullOrEmpty(updateOrder.Address))
            existingOrder.Address = updateOrder.Address;

        if (updateOrder.Phone.HasValue)
            existingOrder.Phone = updateOrder.Phone;

        if (updateOrder.UserId.HasValue)
            existingOrder.UserId = updateOrder.UserId;

        if (updateOrder.BuildId.HasValue)
            existingOrder.BuildId = updateOrder.BuildId;

        await _context.SaveChangesAsync();
        return existingOrder;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return false;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            BuildId = order.BuildId,
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            PaymentMethod = order.PaymentMethod,
            Phone = order.Phone?.ToString(),
            Address = order.Address,
            CreatedAt = order.CreatedAt
        };
    }

    public static List<OrderResponse> ToResponseList(List<Order> orders)
    {
        return orders.Select(ToResponse).ToList();
    }
}

// DTOs
public class OrderCreateRequest
{
    public int? UserId { get; set; }
    public int? BuildId { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
}

public class OrderResponse
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? BuildId { get; set; }
    public string? Status { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? CreatedAt { get; set; }
}
