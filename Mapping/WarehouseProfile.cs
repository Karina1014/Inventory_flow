using System;
using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class WarehouseProfile : Profile
{
    public WarehouseProfile()
    {
        CreateMap<CreateWarehouseDto, Warehouse>();
        CreateMap<UpdateWarehouseDto, Warehouse>();
        CreateMap<Warehouse, WarehouseResponseDto>();
    }
}
