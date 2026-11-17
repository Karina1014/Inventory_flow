using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class UpdateInternalRequisitionStatusDto
{
    /// <summary>Código del nuevo estado (ej. EN_REVISION, DESPACHADO, RECHAZADO).</summary>
    [Required(ErrorMessage = "El nuevo estado es requerido")]
    [MaxLength(30)]
    public string NewStatusCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ReviewNote { get; set; }
}
