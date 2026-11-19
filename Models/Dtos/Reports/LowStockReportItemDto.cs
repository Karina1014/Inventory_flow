using System;

namespace Inventory_Flow.Models.Dtos.Reports;

/// <summary>
/// DTO de detalle/renglón para el reporte RDLC de productos bajo stock por sucursal.
/// 100% mapeado desde la base de datos relacional.
/// </summary>
public class LowStockReportItemDto
{
    public int ItemIndex { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int Threshold { get; set; }
    public int DeficitQuantity { get; set; } // Sugerido de reposición (Threshold - CurrentStock)
    public decimal SuggestedPrice { get; set; }
    public string AlertLevel { get; set; } = string.Empty; // "AGOTADO" o "BAJO STOCK"
    public string NearestExpirationDate { get; set; } = "-"; // Fecha de vencimiento del lote más próximo
}
