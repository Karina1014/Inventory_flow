using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateThresholdInvetoryDto
{
    [Required(ErrorMessage = "El umbral es requerido")]
    [Range(0, 999999, ErrorMessage = "El umbral debe ser mayor o igual a 0")]
    public int Threshold { get; set; }

    public bool IsActive { get; set; } = true;
}
