namespace Inventory_Flow.Models.Dtos.Reports;


public class PurchaseOrderReportItemDto
{
    public int ItemIndex { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public string Notes { get; set; } = string.Empty;
}
