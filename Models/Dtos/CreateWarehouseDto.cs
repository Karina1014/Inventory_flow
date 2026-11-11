using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateWarehouseDto
{
    [Required(ErrorMessage = "El Campo Nombre es requerido")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Campo Dirección es requerido")]
    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
