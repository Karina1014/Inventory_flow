using System;

namespace Inventory_Flow.Models.Dtos;

public class InventoryMovementResponseDto
{
    public int IdInventoryMovement { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}
