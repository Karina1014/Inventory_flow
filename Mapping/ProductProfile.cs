using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<Product, CreateProductDto>().ReverseMap();
        CreateMap<Product, UpdateProductDto>().ReverseMap();
        CreateMap<Product, ProductResponseDto>().ReverseMap();
    }
}
