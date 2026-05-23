using AutoMapper;
using CoffeeShop.Application.DTOs.Products;
using CoffeeShop.Application.Interfaces;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Exceptions;
using CoffeeShop.Domain.Interfaces;

namespace CoffeeShop.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAvailableAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetAvailableAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ProductResponse>>(products);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null || !product.IsAvailable)
        {
            throw new NotFoundException("Product", id);
        }

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetByCategoryAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(category, cancellationToken);
        return _mapper.Map<IReadOnlyList<ProductResponse>>(products);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = _mapper.Map<Product>(request);
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException("Product", id);
        }

        _mapper.Map(request, product);
        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException("Product", id);
        }

        await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
