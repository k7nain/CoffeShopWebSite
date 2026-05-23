using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.DTOs.Admin;

public class UserListItemResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
