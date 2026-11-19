using System;

namespace Inventory_Flow.Models.Dtos.Reports;

/// <summary>
/// DTO de cabecera para el reporte RDLC de productos bajo stock por sucursal.
/// 100% mapeado desde la base de datos relacional.
/// </summary>
public class LowStockReportHeaderDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyRuc { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;

    public string GeneratedAt { get; set; } = string.Empty;
    public int ActiveThreshold { get; set; }

    public int TotalCriticalCount { get; set; } // Stock = 0
    public int TotalWarningCount { get; set; }  // 0 < Stock <= Threshold
    public int TotalAlertCount { get; set; }    // Total de productos en alerta

    public string GeneratedBy { get; set; } = string.Empty;
}
