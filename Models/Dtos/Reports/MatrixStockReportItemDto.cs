using System;

namespace Inventory_Flow.Models.Dtos.Reports;

public class MatrixStockReportItemDto
{
    public int ItemIndex { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; } = string.Empty; // "VIGENTE", "PRÓXIMO A VENCER", "VENCIDO"
}
