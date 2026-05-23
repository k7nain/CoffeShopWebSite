using CoffeeShop.Application.DTOs.Products;
using CoffeeShop.Application.Interfaces;
using MediatR;

namespace CoffeeShop.Application.Features.Products.Queries;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductResponse>>
{
    private readonly IProductService _productService;

    public GetProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public Task<IReadOnlyList<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        => _productService.GetAllAvailableAsync(cancellationToken);
}
