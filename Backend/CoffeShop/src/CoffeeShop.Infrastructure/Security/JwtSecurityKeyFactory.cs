using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CoffeeShop.Infrastructure.Security;

public static class JwtSecurityKeyFactory
{
    public const string KeyId = "CoffeeShopSymmetricKeyV1";

    public static SymmetricSecurityKey Create(string secretKey)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        {
            KeyId = KeyId
        };
    }

    public static SigningCredentials CreateSigningCredentials(string secretKey)
    {
        return new SigningCredentials(
            Create(secretKey),
            SecurityAlgorithms.HmacSha256);
    }
}
