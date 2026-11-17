using System;
using System.Collections.Generic;

namespace Inventory_Flow.Models.Dtos;

public class PurchaseOrderDto
{
    public int IdPurchaseOrder { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }

    // Proveedor
    public int IdSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierRuc { get; set; } = string.Empty;

    // Bodega destino
    public int IdDestinationWarehouse { get; set; }
    public string DestinationWarehouseName { get; set; } = string.Empty;

    // Usuario creador
    public int IdUser { get; set; }
    public string UserName { get; set; } = string.Empty;

    // Tipo de compra
    public int IdPurchaseOrderType { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public int TypePriorityLevel { get; set; }

    // Estado del workflow
    public int IdPurchaseOrderStatus { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusBadgeColor { get; set; } = string.Empty;
    public bool StatusIsFinal { get; set; }

    // Montos financieros
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string Observation { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryDate { get; set; }

    // Trazabilidad de aprobación financiera
    public DateTime? FinancialApprovalDate { get; set; }
    public int? IdFinancialApprovalUser { get; set; }
    public string? FinancialApprovalUserName { get; set; }
    public string? FinancialApprovalNote { get; set; }

    public int TotalItemsCount { get; set; }
    public List<PurchaseOrderDetailDto> Details { get; set; } = new();
}
