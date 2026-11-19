using System;
using System.Collections.Generic;

namespace Inventory_Flow.Models.Dtos.Reports;

/// <summary>
/// DTO estructurado para respuesta JSON consumida por n8n antes de disparar el webhook de Evolution API.
/// </summary>
public class LowStockReportSummaryDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;
    public int ActiveThreshold { get; set; }
    public int TotalAlerts { get; set; }
    public int CriticalCount { get; set; }
    public int WarningCount { get; set; }
    public string GeneratedAt { get; set; } = string.Empty;
    public string PdfDownloadUrl { get; set; } = string.Empty;
    public List<LowStockAlertItemDto> CriticalItems { get; set; } = new();
}

public class LowStockAlertItemDto
{
    public string Code { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int Threshold { get; set; }
    public string Status { get; set; } = string.Empty;
}
