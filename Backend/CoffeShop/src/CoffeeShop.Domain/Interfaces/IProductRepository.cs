using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Domain.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IReadOnlyList<Product>> GetAvailableAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(ProductCategory category, CancellationToken cancellationToken = default);
}
