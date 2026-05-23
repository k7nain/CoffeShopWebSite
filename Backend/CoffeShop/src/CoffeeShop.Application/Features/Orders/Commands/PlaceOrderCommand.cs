using CoffeeShop.Application.DTOs.Orders;
using MediatR;

namespace CoffeeShop.Application.Features.Orders.Commands;

public record PlaceOrderCommand(Guid UserId, CreateOrderRequest Request) : IRequest<OrderResponse>;
