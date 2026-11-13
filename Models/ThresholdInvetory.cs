using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class ThresholdInvetory
{
    [Key]
    public int IdThresholdInvetory { get; set; }

    public int Threshold { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

