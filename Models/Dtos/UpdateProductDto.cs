using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class UpdateProductDto
{
    [Required(ErrorMessage = "El Campo Código es requerido")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Campo Nombre es requerido")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(0, 9999999.99, ErrorMessage = "El Campo Precio Sugerido debe ser mayor o igual a 0")]
    public decimal SuggestedPrice { get; set; }

    public bool Iva { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "El Campo IdCategory es requerido")]
    public int IdCategory { get; set; }
}
