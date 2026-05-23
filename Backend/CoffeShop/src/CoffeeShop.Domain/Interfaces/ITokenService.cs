using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Domain.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    (string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry) CreateTokens(User user);
}
