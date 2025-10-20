using PRM_Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace PRM_Backend.Services;

public class BuildService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BuildService> _logger;

    public BuildService(AppDbContext context, ILogger<BuildService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Build>> GetAllBuildsAsync()
    {
        return await _context.Builds
            .Include(b => b.User)
            .Include(b => b.Items)
                .ThenInclude(i => i.ProductPrice)
                    .ThenInclude(pp => pp.Product)
            .ToListAsync();
    }

    public async Task<Build?> GetBuildByIdAsync(int id)
    {
        return await _context.Builds
            .Include(b => b.User)
            .Include(b => b.Items)
                .ThenInclude(i => i.ProductPrice)
                    .ThenInclude(pp => pp.Product)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Build> CreateBuildAsync(BuildCreateRequest request)
    {
        var build = new Build
        {
            UserId = request.UserId,
            Name = request.Name,
            TotalPrice = request.TotalPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Builds.Add(build);
        await _context.SaveChangesAsync();

        // Thêm các items nếu có
        if (request.Items != null && request.Items.Any())
        {
            foreach (var item in request.Items)
            {
                var buildItem = new BuildItem
                {
                    BuildId = build.Id,
                    ProductPriceId = item.ProductPriceId,
                    Quantity = item.Quantity ?? 1
                };
                _context.BuildItems.Add(buildItem);
            }
            await _context.SaveChangesAsync();
        }

        return await GetBuildByIdAsync(build.Id) ?? build;
    }

    public async Task<Build?> UpdateBuildAsync(int id, Build updateBuild)
    {
        var existingBuild = await _context.Builds.FindAsync(id);
        if (existingBuild == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(updateBuild.Name))
            existingBuild.Name = updateBuild.Name;
        
        if (updateBuild.TotalPrice.HasValue)
            existingBuild.TotalPrice = updateBuild.TotalPrice;

        if (updateBuild.UserId.HasValue)
            existingBuild.UserId = updateBuild.UserId;

        await _context.SaveChangesAsync();
        return existingBuild;
    }

    public async Task<bool> DeleteBuildAsync(int id)
    {
        var build = await _context.Builds.FindAsync(id);
        if (build == null)
        {
            return false;
        }

        _context.Builds.Remove(build);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Build>> GetBuildsByUserAsync(int userId)
    {
        return await _context.Builds
            .Include(b => b.User)
            .Include(b => b.Items)
                .ThenInclude(i => i.ProductPrice)
                    .ThenInclude(pp => pp.Product)
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }
}

// DTOs
public class BuildCreateRequest
{
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? TotalPrice { get; set; }
    public List<BuildCreateItem>? Items { get; set; }
}

public class BuildCreateItem
{
    public int ProductPriceId { get; set; }
    public int? Quantity { get; set; }
}

public class BuildResponse
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? TotalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<BuildItemResponse> Items { get; set; } = new();
}

public class BuildItemResponse
{
    public int Id { get; set; }
    public int ProductPriceId { get; set; }
    public int Quantity { get; set; }
    public ProductPrice? ProductPrice { get; set; }
}

public class BuildDetailResponse
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? TotalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
    public User? User { get; set; }
    public List<BuildItemResponse> Items { get; set; } = new();
}
