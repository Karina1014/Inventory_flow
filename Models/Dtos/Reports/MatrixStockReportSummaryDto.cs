using System;
using System.Collections.Generic;

namespace Inventory_Flow.Models.Dtos.Reports;

public class MatrixStockReportSummaryDto
{
    public string WarehouseName { get; set; } = string.Empty;
    public string GeneratedAt { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int TotalLots { get; set; }
    public int TotalUnits { get; set; }
    public decimal TotalValue { get; set; }
    public int ExpiringSoonLotsCount { get; set; }
    public int ExpiredLotsCount { get; set; }
    public string PdfDownloadUrl { get; set; } = string.Empty;
    public List<MatrixStockExpiringLotDto> TopExpiringLots { get; set; } = new();
}

public class MatrixStockExpiringLotDto
{
    public string Product { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
}
