using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateInternalRequisitionDto
{
    [Required(ErrorMessage = "La sucursal solicitante es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La sucursal solicitante es obligatoria")]
    public int IdRequestingWarehouse { get; set; }

    /// <summary>Matriz o sucursal proveedora (si no se envía, se usa la matriz Carapungo).</summary>
    public int? IdFulfillingWarehouse { get; set; }

    [Required(ErrorMessage = "El tipo de requisición es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de requisición es obligatorio")]
    public int IdRequisitionType { get; set; }

    [MaxLength(500)]
    public string Observation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe agregar al menos un producto a la requisición")]
    [MinLength(1, ErrorMessage = "Debe agregar al menos un producto a la requisición")]
    public List<CreateInternalRequisitionDetailDto> Details { get; set; } = new();
}
