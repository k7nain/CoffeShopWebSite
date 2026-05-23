using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Interfaces;
using CoffeeShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetFilteredAsync(
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= fromDate.Value.ToUniversalTime());
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= toDate.Value.ToUniversalTime());
        }

        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<(int TotalOrders, decimal TotalRevenue)> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var deliveredOrders = DbSet.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered);

        var totalOrders = await deliveredOrders.CountAsync(cancellationToken);
        var totalRevenue = await deliveredOrders.SumAsync(o => o.TotalAmount, cancellationToken);

        return (totalOrders, totalRevenue);
    }

    public async Task<IReadOnlyList<(Guid ProductId, string ProductName, int TotalQuantity)>> GetTopProductsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var results = await Context.OrderItems.AsNoTracking()
            .Where(oi => oi.Order.Status == OrderStatus.Delivered)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new
            {
                g.Key.ProductId,
                ProductName = g.Key.Name,
                TotalQuantity = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(count)
            .ToListAsync(cancellationToken);

        return results
            .Select(r => (r.ProductId, r.ProductName, r.TotalQuantity))
            .ToList();
    }
}
