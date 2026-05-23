namespace CoffeeShop.Application.DTOs.Orders;

public class CreateOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
