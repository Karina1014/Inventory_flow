using System;

namespace Inventory_Flow.Models.Dtos;

public class MovementTypeDto
{
    public int IdMovementType { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}
