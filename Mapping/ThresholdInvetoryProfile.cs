using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class ThresholdInvetoryProfile : Profile
{
    public ThresholdInvetoryProfile()
    {
        CreateMap<ThresholdInvetory, ThresholdInvetoryDto>().ReverseMap();
        CreateMap<CreateThresholdInvetoryDto, ThresholdInvetory>();
        CreateMap<UpdateThresholdInvetoryDto, ThresholdInvetory>();
    }
}

