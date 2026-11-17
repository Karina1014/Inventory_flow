using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class InternalRequisitionProfile : Profile
{
    public InternalRequisitionProfile()
    {
        CreateMap<RequisitionStatus, RequisitionStatusDto>().ReverseMap();
        CreateMap<RequisitionType, RequisitionTypeDto>().ReverseMap();

        CreateMap<InternalRequisition, InternalRequisitionDto>()
            .ForMember(d => d.RequestingWarehouseName, o => o.MapFrom(s => s.RequestingWarehouse != null ? s.RequestingWarehouse.Name : string.Empty))
            .ForMember(d => d.FulfillingWarehouseName, o => o.MapFrom(s => s.FulfillingWarehouse != null ? s.FulfillingWarehouse.Name : string.Empty))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? (s.User.Name ?? s.User.Username) : string.Empty))
            .ForMember(d => d.ReviewedByUserName, o => o.MapFrom(s => s.ReviewedByUser != null ? (s.ReviewedByUser.Name ?? s.ReviewedByUser.Username) : null))
            .ForMember(d => d.TypeCode, o => o.MapFrom(s => s.RequisitionType != null ? s.RequisitionType.Code : string.Empty))
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.RequisitionType != null ? s.RequisitionType.Name : string.Empty))
            .ForMember(d => d.TypePriorityLevel, o => o.MapFrom(s => s.RequisitionType != null ? s.RequisitionType.PriorityLevel : 0))
            .ForMember(d => d.TypeMaxResponseHours, o => o.MapFrom(s => s.RequisitionType != null ? s.RequisitionType.MaxResponseHours : 0))
            .ForMember(d => d.StatusCode, o => o.MapFrom(s => s.RequisitionStatus != null ? s.RequisitionStatus.Code : string.Empty))
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.RequisitionStatus != null ? s.RequisitionStatus.Name : string.Empty))
            .ForMember(d => d.StatusBadgeColor, o => o.MapFrom(s => s.RequisitionStatus != null ? s.RequisitionStatus.BadgeColor : "secondary"))
            .ForMember(d => d.StatusIsFinal, o => o.MapFrom(s => s.RequisitionStatus != null && s.RequisitionStatus.IsFinal));

        CreateMap<InternalRequisitionDetail, InternalRequisitionDetailDto>()
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.Code : string.Empty))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.ProductCategory, o => o.MapFrom(s => s.Product != null && s.Product.Category != null ? s.Product.Category.Name : string.Empty));
    }
}
