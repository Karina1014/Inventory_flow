using System;
using System.Collections.Generic;

namespace Inventory_Flow.Models.Dtos.Reports;

public class ExpiringLotDto
{
    public int IdLot { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int IdWarehouse { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int DaysRemaining { get; set; }
    public string UrgencyLevel { get; set; } = string.Empty;
}

public class WarehouseExpiringLotsSummaryDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int TotalLotsAtRisk { get; set; }
    public int TotalUnitsAtRisk { get; set; }
    public List<ExpiringLotDto> ExpiringLots { get; set; } = new();
    public string FormattedWhatsAppMessage { get; set; } = string.Empty;
}

public class ConsolidatedExpiringLotsReportDto
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public int DaysAhead { get; set; } = 30;
    public int TotalWarehousesWithExpiringLots { get; set; }
    public int GrandTotalLotsAtRisk { get; set; }
    public int GrandTotalUnitsAtRisk { get; set; }
    public List<WarehouseExpiringLotsSummaryDto> Warehouses { get; set; } = new();
    public string MatrizConsolidatedWhatsAppMessage { get; set; } = string.Empty;
}
