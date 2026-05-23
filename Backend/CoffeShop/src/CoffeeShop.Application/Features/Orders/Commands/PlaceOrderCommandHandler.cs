using CoffeeShop.Application.DTOs.Orders;
using CoffeeShop.Application.Interfaces;
using MediatR;

namespace CoffeeShop.Application.Features.Orders.Commands;

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderResponse>
{
    private readonly IOrderService _orderService;

    public PlaceOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public Task<OrderResponse> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        => _orderService.PlaceOrderAsync(request.UserId, request.Request, cancellationToken);
}
