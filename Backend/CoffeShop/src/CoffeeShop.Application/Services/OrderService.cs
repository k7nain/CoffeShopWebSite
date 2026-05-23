using AutoMapper;
using CoffeeShop.Application.DTOs.Orders;
using CoffeeShop.Application.Interfaces;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Exceptions;
using CoffeeShop.Domain.Interfaces;

namespace CoffeeShop.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderResponse> PlaceOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        var orderItems = new List<OrderItem>();
        decimal total = 0;

        foreach (var item in request.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null || !product.IsAvailable)
            {
                throw new BusinessException($"Product '{item.ProductId}' is not available.");
            }

            var lineTotal = product.Price * item.Quantity;
            total += lineTotal;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Product = product
            });
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = total,
            CreatedAt = now,
            UpdatedAt = now,
            OrderItems = orderItems
        };

        foreach (var orderItem in orderItems)
        {
            orderItem.OrderId = order.Id;
        }

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Orders.GetByIdWithDetailsAsync(order.Id, cancellationToken);
        return _mapper.Map<OrderResponse>(created!);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderResponse>>(orders);
    }

    public async Task<OrderResponse> GetOrderByIdAsync(Guid orderId, Guid userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("Order", orderId);
        }

        if (!isAdmin && order.UserId != userId)
        {
            throw new UnauthorizedException("You do not have access to this order.");
        }

        return _mapper.Map<OrderResponse>(order);
    }

    public async Task<OrderResponse> CancelOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("Order", orderId);
        }

        if (order.UserId != userId)
        {
            throw new UnauthorizedException("You do not have access to this order.");
        }

        if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            throw new BusinessException($"Order cannot be cancelled in '{order.Status}' status.");
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderResponse>(order);
    }
}
