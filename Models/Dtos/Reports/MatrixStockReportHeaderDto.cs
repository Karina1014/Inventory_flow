using System;

namespace Inventory_Flow.Models.Dtos.Reports;

public class MatrixStockReportHeaderDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyRuc { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;

    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;

    public string GeneratedAt { get; set; } = string.Empty;
    public int ExpirationAlertDays { get; set; }

    public int TotalProductsCount { get; set; }
    public int TotalLotsCount { get; set; }
    public int TotalUnitsCount { get; set; }
    public decimal TotalValuedAmount { get; set; }

    public int ExpiringSoonLotsCount { get; set; }
    public int ExpiredLotsCount { get; set; }
    public int ValidLotsCount { get; set; }
    public int LowStockProductsCount { get; set; }
    public int ActiveThreshold { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;
}
