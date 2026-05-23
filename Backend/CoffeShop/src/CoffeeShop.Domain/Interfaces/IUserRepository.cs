using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> AnyAdminExistsAsync(CancellationToken cancellationToken = default);
}
