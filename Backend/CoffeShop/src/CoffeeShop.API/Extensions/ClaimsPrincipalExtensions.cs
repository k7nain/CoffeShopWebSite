using System.Security.Claims;

namespace CoffeeShop.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var userId))
        {
            throw new UnauthorizedAccessException("User identifier is missing from token.");
        }

        return userId;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole("Admin");
    }
}
