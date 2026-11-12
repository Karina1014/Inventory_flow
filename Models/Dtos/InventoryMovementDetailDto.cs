using System;

namespace Inventory_Flow.Models.Dtos;

public class InventoryMovementDetailDto
{
    public int IdInventoryMovementDetail { get; set; }
    public int IdInventoryMovement { get; set; }
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int? IdLot { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Subtotal => Quantity * UnitCost;
}

