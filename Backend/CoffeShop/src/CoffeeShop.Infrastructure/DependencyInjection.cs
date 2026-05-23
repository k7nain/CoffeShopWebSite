using CoffeeShop.Application.Interfaces;
using CoffeeShop.Domain.Interfaces;
using CoffeeShop.Infrastructure.Configuration;
using CoffeeShop.Infrastructure.Persistence;
using CoffeeShop.Infrastructure.Repositories;
using CoffeeShop.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var useInMemory = configuration.GetValue<bool?>("UseInMemoryDatabase")
            ?? configuration.GetValue("ASPNETCORE_ENVIRONMENT", "") == "Development";
        var connectionString = DatabaseConnectionResolver.Resolve(configuration);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("CoffeeShopDb");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "PostgreSQL connection string is missing. Set DATABASE_URL (Railway) or ConnectionStrings:DefaultConnection.");
                }

                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
