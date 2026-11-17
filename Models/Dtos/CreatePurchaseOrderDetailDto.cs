using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreatePurchaseOrderDetailDto
{
    [Required(ErrorMessage = "Debe seleccionar un producto")]
    public int IdProduct { get; set; }

    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, 1000000, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "El precio unitario de compra es requerido")]
    [Range(0.00, 9999999.99, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
    public decimal UnitPrice { get; set; }

    public decimal TaxPercentage { get; set; } = 15.00m;

    [MaxLength(200)]
    public string Notes { get; set; } = string.Empty;

    public int? IdInternalRequisition { get; set; }
}
