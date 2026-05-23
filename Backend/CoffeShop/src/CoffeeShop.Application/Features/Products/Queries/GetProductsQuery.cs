using CoffeeShop.Application.DTOs.Products;
using MediatR;

namespace CoffeeShop.Application.Features.Products.Queries;

public record GetProductsQuery : IRequest<IReadOnlyList<ProductResponse>>;
