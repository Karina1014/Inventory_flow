using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class UpdateInventoryMovementDto
{
    [MaxLength(255, ErrorMessage = "El motivo no puede exceder 255 caracteres")]
    public string Observation { get; set; } = string.Empty;

    public int? ReferenceId { get; set; }

    [MaxLength(50)]
    public string? ReferenceSource { get; set; }
}

