using CoffeeShop.Application.DTOs.Admin;
using CoffeeShop.Application.DTOs.Orders;
using CoffeeShop.Application.Interfaces;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var orders = await _adminService.GetAllOrdersAsync(status, fromDate, toDate, cancellationToken);
        return Ok(orders);
    }

    [HttpPut("orders/{id:guid}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _adminService.UpdateOrderStatusAsync(id, request, cancellationToken);
        return Ok(order);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _adminService.GetDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<UserListItemResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _adminService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }
}
