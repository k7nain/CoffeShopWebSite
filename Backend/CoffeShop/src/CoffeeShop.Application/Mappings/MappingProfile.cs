using AutoMapper;
using CoffeeShop.Application.DTOs.Admin;
using CoffeeShop.Application.DTOs.Orders;
using CoffeeShop.Application.DTOs.Products;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductResponse>();
        CreateMap<CreateProductRequest, Product>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.OrderItems, opt => opt.Ignore());
        CreateMap<UpdateProductRequest, Product>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.OrderItems, opt => opt.Ignore());

        CreateMap<OrderItem, OrderItemResponse>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));

        CreateMap<Order, OrderResponse>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.OrderItems));

        CreateMap<User, UserListItemResponse>();
    }
}
