using CoffeeShop.Application.Configuration;
using CoffeeShop.Application.Interfaces;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace CoffeeShop.Infrastructure.Seeding;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger,
        AdminSetupSettings adminSetup)
    {
        if (context.Database.IsNpgsql())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        await EnsureDefaultAdminAsync(context, passwordHasher, logger, adminSetup);

        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Espresso",
                    Description = "Rich and bold single shot espresso.",
                    Price = 2.50m,
                    Category = ProductCategory.Coffee,
                    ImageUrl = "https://example.com/images/espresso.jpg",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Cappuccino",
                    Description = "Espresso with steamed milk and foam.",
                    Price = 4.00m,
                    Category = ProductCategory.Coffee,
                    ImageUrl = "https://example.com/images/cappuccino.jpg",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Green Tea",
                    Description = "Light and refreshing green tea.",
                    Price = 3.00m,
                    Category = ProductCategory.Tea,
                    ImageUrl = "https://example.com/images/green-tea.jpg",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Croissant",
                    Description = "Buttery flaky pastry.",
                    Price = 3.50m,
                    Category = ProductCategory.Food,
                    ImageUrl = "https://example.com/images/croissant.jpg",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Chocolate Cake",
                    Description = "Decadent chocolate layer cake slice.",
                    Price = 5.50m,
                    Category = ProductCategory.Dessert,
                    ImageUrl = "https://example.com/images/chocolate-cake.jpg",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Products.AddRangeAsync(products);
            logger.LogInformation("Seeded {Count} sample products.", products.Count);
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDefaultAdminAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger,
        AdminSetupSettings adminSetup)
    {
        var email = adminSetup.DefaultAdminEmail.Trim().ToLowerInvariant();
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (admin is null)
        {
            admin = new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FullName = "System Admin",
                Email = email,
                PasswordHash = passwordHasher.Hash(adminSetup.DefaultAdminPassword),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(admin);
            logger.LogInformation(
                "Created default admin: {Email} / {Password}",
                email,
                adminSetup.DefaultAdminPassword);
            return;
        }

        var updated = false;

        if (admin.Role != UserRole.Admin)
        {
            admin.Role = UserRole.Admin;
            updated = true;
            logger.LogWarning("User {Email} was promoted to Admin role.", email);
        }

        if (adminSetup.ResetDefaultAdminPasswordOnStartup)
        {
            admin.PasswordHash = passwordHasher.Hash(adminSetup.DefaultAdminPassword);
            updated = true;
            logger.LogInformation(
                "Reset default admin password for {Email} (password: {Password})",
                email,
                adminSetup.DefaultAdminPassword);
        }

        if (updated)
        {
            context.Users.Update(admin);
        }
        else
        {
            logger.LogInformation("Default admin already exists: {Email}", email);
        }
    }
}
