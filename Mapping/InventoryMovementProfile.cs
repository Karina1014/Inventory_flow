using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class InventoryMovementProfile : Profile
{
    public InventoryMovementProfile()
    {
        CreateMap<InventoryMovement, InventoryMovementDto>().ReverseMap();
        CreateMap<InventoryMovement, CreateInventoryMovementDto>().ReverseMap();
        CreateMap<InventoryMovement, UpdateInventoryMovementDto>().ReverseMap();
        CreateMap<InventoryMovement, InventoryMovementResponseDto>().ReverseMap();
    }
}
