using CoffeeShop.Application.DTOs.Admin;
using CoffeeShop.Application.DTOs.Orders;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyList<OrderResponse>> GetAllOrdersAsync(
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserListItemResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
