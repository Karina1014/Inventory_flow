using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class MovementTypeProfile : Profile
{
    public MovementTypeProfile()
    {
        CreateMap<MovementType, MovementTypeDto>().ReverseMap();
        CreateMap<MovementType, CreateMovementTypeDto>().ReverseMap();
        CreateMap<MovementType, UpdateMovementTypeDto>().ReverseMap();
        CreateMap<MovementType, MovementTypeResponseDto>().ReverseMap();
    }
}
