using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateInventoryMovementDetailDto
{
    [Required(ErrorMessage = "El producto es requerido")]
    public int IdProduct { get; set; }

    public int? IdLot { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; } = 1;

    [Range(0, 9999999.99, ErrorMessage = "El costo unitario debe ser mayor o igual a 0")]
    public decimal UnitCost { get; set; }

    // Propiedades UI para feedback instantáneo al usuario
    public bool HasIva { get; set; }
    public int AvailableStock { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
}
