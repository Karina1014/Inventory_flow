using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateRoleDto
{
    [Required(ErrorMessage = "El Campo Nombre es requerido")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
