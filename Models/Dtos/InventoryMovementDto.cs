using System;
using System.Collections.Generic;

namespace Inventory_Flow.Models.Dtos;

public class InventoryMovementDto
{
    public int IdInventoryMovement { get; set; }
    public DateTime MovementDate { get; set; }
    public int IdMovementType { get; set; }
    public string MovementTypeCode { get; set; } = string.Empty;
    public string MovementTypeMode { get; set; } = string.Empty;
    public string MovementTypeDescription { get; set; } = string.Empty;

    public int? IdOriginWarehouse { get; set; }
    public string? OriginWarehouseName { get; set; }

    public int? IdDestinationWarehouse { get; set; }
    public string? DestinationWarehouseName { get; set; }

    public int? IdUser { get; set; }
    public string UserName { get; set; } = string.Empty;

    public string Observation { get; set; } = string.Empty;
    public int? ReferenceId { get; set; }
    public string? ReferenceSource { get; set; }

    public int DetailsCount { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }

    public List<InventoryMovementDetailDto> Details { get; set; } = new();

    /// <summary>Representación de flujo para la tabla (ej. "1 -> -", "1 -> 2", "- -> 1")</summary>
    public string WarehousesFlowDisplay
    {
        get
        {
            var origin = IdOriginWarehouse.HasValue ? IdOriginWarehouse.Value.ToString() : "-";
            var dest = IdDestinationWarehouse.HasValue ? IdDestinationWarehouse.Value.ToString() : "-";
            return $"{origin} -> {dest}";
        }
    }
}

