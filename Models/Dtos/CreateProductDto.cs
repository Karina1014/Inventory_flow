using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateProductDto
{
    [Required(ErrorMessage = "El Campo Nombre es requerido")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Campo IdCategory es requerido")]
    public int IdCategory { get; set; }
}
