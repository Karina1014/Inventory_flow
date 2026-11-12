using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class InventoryMovementProfile : Profile
{
    public InventoryMovementProfile()
    {
        CreateMap<InventoryMovement, InventoryMovementDto>()
            .ForMember(dest => dest.MovementTypeCode, opt => opt.MapFrom(src => src.MovementType != null ? src.MovementType.Code : string.Empty))
            .ForMember(dest => dest.MovementTypeMode, opt => opt.MapFrom(src => src.MovementType != null ? src.MovementType.Mode : string.Empty))
            .ForMember(dest => dest.MovementTypeDescription, opt => opt.MapFrom(src => src.MovementType != null ? src.MovementType.Description : string.Empty))
            .ForMember(dest => dest.OriginWarehouseName, opt => opt.MapFrom(src => src.OriginWarehouse != null ? src.OriginWarehouse.Name : null))
            .ForMember(dest => dest.DestinationWarehouseName, opt => opt.MapFrom(src => src.DestinationWarehouse != null ? src.DestinationWarehouse.Name : null))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : "Sistema"))
            .ForMember(dest => dest.DetailsCount, opt => opt.MapFrom(src => src.Details != null ? src.Details.Count : 0))
            .ForMember(dest => dest.TotalQuantity, opt => opt.MapFrom(src => src.TotalQuantity))
            .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.TotalCost));

        CreateMap<InventoryMovementDetail, InventoryMovementDetailDto>()
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.LotNumber, opt => opt.MapFrom(src => src.Lot != null ? src.Lot.LotNumber : null))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.Lot != null ? (DateTime?)src.Lot.ExpirationDate : null));

        CreateMap<CreateInventoryMovementDto, InventoryMovement>()
            .ForMember(dest => dest.MovementDate, opt => opt.MapFrom(src => src.MovementDate ?? DateTime.Now))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

        CreateMap<CreateInventoryMovementDetailDto, InventoryMovementDetail>();

        CreateMap<UpdateInventoryMovementDto, InventoryMovement>();
    }
}

