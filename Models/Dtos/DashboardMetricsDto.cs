using System;
using System.Collections.Generic;

namespace Inventory_Flow.Models.Dtos;

public class CategoryStockDto
{
    public int IdCategory { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalUnits { get; set; }
    public double Percentage { get; set; }
    public bool IsLowStock { get; set; }
}

public class RecentActivityDto
{
    public int IdMovement { get; set; }
    public DateTime Date { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
    public string MovementTypeCode { get; set; } = string.Empty;
    public string MovementTypeMode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsAlert { get; set; }
}

public class ExpiringLotDto
{
    public int IdLot { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public int DaysToExpire { get; set; }
    public int Quantity { get; set; }
}

public class MonthlyMovementTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public string FullMonthName { get; set; } = string.Empty;
    public int EntriesUnits { get; set; }
    public int ExitsUnits { get; set; }
    public decimal EntriesValue { get; set; }
    public decimal ExitsValue { get; set; }
    public double EntriesHeightPercent { get; set; }
    public double ExitsHeightPercent { get; set; }
}

public class TopSellingProductDto
{
    public int Rank { get; set; }
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalUnitsSold { get; set; }
    public decimal TotalSalesValue { get; set; }
    public int CurrentStock { get; set; }
    public double SharePercentage { get; set; }
}

public class DashboardMetricsDto
{
    public decimal TotalInventoryValue { get; set; }
    public int TodayMovementsCount { get; set; }
    public int ActiveWarehousesCount { get; set; }
    public int LowStockProductsCount { get; set; }
    public int TotalProductsCount { get; set; }
    public int TotalLotsCount { get; set; }

    /// <summary>Umbral de stock vigente (IsActive = true) usado para las alertas.</summary>
    public int ActiveThreshold { get; set; }

    /// <summary>Ventana de aviso de caducidad en días.</summary>
    public int ExpirationAlertDays { get; set; }
    public int ExpiringLotsCount { get; set; }
    public int ExpiredLotsCount { get; set; }

    public int TotalUnitsSoldPeriod { get; set; }
    public decimal TotalSalesValuePeriod { get; set; }
    public int PendingRequisitionsCount { get; set; }
    public int PendingPurchaseOrdersCount { get; set; }

    public List<MonthlyMovementTrendDto> MonthlyTrends { get; set; } = new();
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
    public List<ExpiringLotDto> ExpiringSoonLots { get; set; } = new();

    public List<CategoryStockDto> StockByCategory { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

