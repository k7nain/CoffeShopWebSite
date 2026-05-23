using CoffeeShop.Application.DTOs.Orders;

namespace CoffeeShop.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> PlaceOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponse>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OrderResponse> GetOrderByIdAsync(Guid orderId, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<OrderResponse> CancelOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);
}
