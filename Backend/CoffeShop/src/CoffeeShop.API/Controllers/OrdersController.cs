using CoffeeShop.API.Extensions;
using CoffeeShop.Application.DTOs.Orders;
using CoffeeShop.Application.Features.Orders.Commands;
using CoffeeShop.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IMediator _mediator;

    public OrdersController(IOrderService orderService, IMediator mediator)
    {
        _orderService = orderService;
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderResponse>> PlaceOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(
            new PlaceOrderCommand(User.GetUserId(), request),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetMyOrders(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetMyOrdersAsync(User.GetUserId(), cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(
            id,
            User.GetUserId(),
            User.IsAdmin(),
            cancellationToken);
        return Ok(order);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderResponse>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.CancelOrderAsync(id, User.GetUserId(), cancellationToken);
        return Ok(order);
    }
}
