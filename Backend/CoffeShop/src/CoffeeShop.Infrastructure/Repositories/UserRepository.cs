using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Interfaces;
using CoffeeShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            u => u.RefreshToken == refreshToken,
            cancellationToken);
    }

    public async Task<bool> AnyAdminExistsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Role == Domain.Enums.UserRole.Admin, cancellationToken);
    }
}
