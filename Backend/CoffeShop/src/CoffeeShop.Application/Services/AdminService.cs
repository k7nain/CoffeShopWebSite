using AutoMapper;
using CoffeeShop.Application.DTOs.Admin;
using CoffeeShop.Application.DTOs.Orders;
using CoffeeShop.Application.Interfaces;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Exceptions;
using CoffeeShop.Domain.Interfaces;

namespace CoffeeShop.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllOrdersAsync(
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetFilteredAsync(status, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderResponse>>(orders);
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("Order", orderId);
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new BusinessException("Cannot update status of a cancelled order.");
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderResponse>(order);
    }

    public async Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var (totalOrders, totalRevenue) = await _unitOfWork.Orders.GetDashboardStatsAsync(cancellationToken);
        var topProducts = await _unitOfWork.Orders.GetTopProductsAsync(5, cancellationToken);

        return new DashboardResponse
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TopProducts = topProducts.Select(p => new TopProductResponse
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                TotalQuantitySold = p.TotalQuantity
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<UserListItemResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<UserListItemResponse>>(users);
    }
}
