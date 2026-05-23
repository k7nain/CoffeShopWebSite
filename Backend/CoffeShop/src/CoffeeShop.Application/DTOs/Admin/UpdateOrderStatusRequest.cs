using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.DTOs.Admin;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}
