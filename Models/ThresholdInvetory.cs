using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class ThresholdInvetory
{
    [Key]
    public int IdThresholdInvetory { get; set; }

    [Required(ErrorMessage = "El umbral es requerido")]
    [Range(0, 999999, ErrorMessage = "El umbral debe ser mayor o igual a 0")]
    public int Threshold { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
