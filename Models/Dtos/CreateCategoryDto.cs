using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "El Campo Nombre es requerido")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Campo Descripción es requerido")]
    public string Description { get; set; } = string.Empty;
}
