namespace Inventory_Flow.Models.Dtos;

public class PurchaseOrderDetailDto
{
    public int IdPurchaseOrderDetail { get; set; }
    public int IdPurchaseOrder { get; set; }
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int? IdInternalRequisition { get; set; }
    public string? InternalRequisitionCode { get; set; }
}
