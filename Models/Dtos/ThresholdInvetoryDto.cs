using System;

namespace Inventory_Flow.Models.Dtos;

public class ThresholdInvetoryDto
{
    public int IdThresholdInvetory { get; set; }
    public int Threshold { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
