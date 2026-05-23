using CoffeeShop.Application.DTOs.Products;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAvailableAsync(CancellationToken cancellationToken = default);
    Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductResponse>> GetByCategoryAsync(ProductCategory category, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
