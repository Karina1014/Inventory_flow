using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class WarehouseProfile : Profile
{
    public WarehouseProfile()
    {
        CreateMap<Warehouse, WarehouseDto>().ReverseMap();
        CreateMap<Warehouse, CreateWarehouseDto>().ReverseMap();
        CreateMap<Warehouse, UpdateWarehouseDto>().ReverseMap();
    }
}
