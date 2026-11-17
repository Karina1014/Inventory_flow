using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreatePurchaseOrderDto
{
    [Required(ErrorMessage = "El proveedor es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
    public int IdSupplier { get; set; }

    public int? IdDestinationWarehouse { get; set; }

    [Required(ErrorMessage = "El tipo de compra es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de orden de compra")]
    public int IdPurchaseOrderType { get; set; }

    [MaxLength(500)]
    public string Observation { get; set; } = string.Empty;

    public DateTime? ExpectedDeliveryDate { get; set; }

    [Required(ErrorMessage = "La orden debe contener al menos un producto")]
    [MinLength(1, ErrorMessage = "La orden debe contener al menos un producto")]
    public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
}
