using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Mapping;

public class PurchaseOrderProfile : Profile
{
    public PurchaseOrderProfile()
    {
        CreateMap<PurchaseOrderStatus, PurchaseOrderStatusDto>().ReverseMap();
        CreateMap<PurchaseOrderType, PurchaseOrderTypeDto>().ReverseMap();

        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.CompanyName : string.Empty))
            .ForMember(d => d.SupplierRuc, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Ruc : string.Empty))
            .ForMember(d => d.DestinationWarehouseName, o => o.MapFrom(s => s.DestinationWarehouse != null ? s.DestinationWarehouse.Name : string.Empty))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? (s.User.Name ?? s.User.Username) : string.Empty))
            .ForMember(d => d.FinancialApprovalUserName, o => o.MapFrom(s => s.FinancialApprovalUser != null ? (s.FinancialApprovalUser.Name ?? s.FinancialApprovalUser.Username) : null))
            .ForMember(d => d.TypeCode, o => o.MapFrom(s => s.PurchaseOrderType != null ? s.PurchaseOrderType.Code : string.Empty))
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.PurchaseOrderType != null ? s.PurchaseOrderType.Name : string.Empty))
            .ForMember(d => d.TypePriorityLevel, o => o.MapFrom(s => s.PurchaseOrderType != null ? s.PurchaseOrderType.PriorityLevel : 0))
            .ForMember(d => d.StatusCode, o => o.MapFrom(s => s.PurchaseOrderStatus != null ? s.PurchaseOrderStatus.Code : string.Empty))
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.PurchaseOrderStatus != null ? s.PurchaseOrderStatus.Name : string.Empty))
            .ForMember(d => d.StatusBadgeColor, o => o.MapFrom(s => s.PurchaseOrderStatus != null ? s.PurchaseOrderStatus.BadgeColor : "secondary"))
            .ForMember(d => d.StatusIsFinal, o => o.MapFrom(s => s.PurchaseOrderStatus != null && s.PurchaseOrderStatus.IsFinal));

        CreateMap<PurchaseOrderDetail, PurchaseOrderDetailDto>()
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.Code : string.Empty))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.ProductCategory, o => o.MapFrom(s => s.Product != null && s.Product.Category != null ? s.Product.Category.Name : string.Empty))
            .ForMember(d => d.InternalRequisitionCode, o => o.MapFrom(s => s.InternalRequisition != null ? s.InternalRequisition.RequisitionCode : null));
    }
}
