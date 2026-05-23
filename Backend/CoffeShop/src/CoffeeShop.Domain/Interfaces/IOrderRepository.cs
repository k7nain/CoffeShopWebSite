using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Domain.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetFilteredAsync(
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);
    Task<(int TotalOrders, decimal TotalRevenue)> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Guid ProductId, string ProductName, int TotalQuantity)>> GetTopProductsAsync(
        int count,
        CancellationToken cancellationToken = default);
}
