using CoffeeShop.Infrastructure.Configuration;
using CoffeeShop.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoffeeShop.API.Configuration;

public class JwtBearerConfiguration : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly JwtSettings _jwtSettings;

    public JwtBearerConfiguration(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var signingKey = JwtSecurityKeyFactory.Create(_jwtSettings.SecretKey);

        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            IssuerSigningKey = signingKey,
            IssuerSigningKeyResolver = (_, _, kid, _) =>
            {
                if (string.IsNullOrEmpty(kid) || kid == JwtSecurityKeyFactory.KeyId)
                {
                    return [signingKey];
                }

                return [JwtSecurityKeyFactory.Create(_jwtSettings.SecretKey)];
            },
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }

                return Task.CompletedTask;
            }
        };
    }
}
