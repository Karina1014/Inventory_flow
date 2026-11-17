using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateInternalRequisitionDetailDto
{
    [Required(ErrorMessage = "Debe seleccionar un producto")]
    public int IdProduct { get; set; }

    [Required(ErrorMessage = "Debe ingresar una cantidad")]
    [Range(1, 100000, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int RequestedQuantity { get; set; }

    [MaxLength(200)]
    public string Note { get; set; } = string.Empty;
}
