namespace CoffeeShop.Application.DTOs.Admin;

public class DashboardResponse
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<TopProductResponse> TopProducts { get; set; } = new();
}

public class TopProductResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
}
