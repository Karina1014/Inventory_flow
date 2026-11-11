using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateInventoryMovementDto
{
    [Required(ErrorMessage = "El Campo Código es requerido")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Campo Modo es requerido")]
    public string Mode { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
