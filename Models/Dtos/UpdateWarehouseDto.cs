using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class UpdateWarehouseDto
{
    [Required]
    public int IdWarehouse { get; set; }

    [Required(ErrorMessage = "El Campo Nombre es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Campo Dirección es requerido")]
    [MaxLength(255, ErrorMessage = "La dirección no puede superar los 255 caracteres")]
    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } 
}
