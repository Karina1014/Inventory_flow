using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class LotProfile : Profile
{
    public LotProfile()
    {
        CreateMap<Lot, LotDto>().ReverseMap();
        CreateMap<Lot, CreateLotDto>().ReverseMap();
        CreateMap<Lot, UpdateLotDto>().ReverseMap();
        CreateMap<Lot, LotResponseDto>().ReverseMap();
    }
}
