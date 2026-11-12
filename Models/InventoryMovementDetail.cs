using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Flow.Models;

public class InventoryMovementDetail
{
    [Key]
    public int IdInventoryMovementDetail { get; set; }

    public int IdInventoryMovement { get; set; }

    [ForeignKey("IdInventoryMovement")]
    public InventoryMovement? InventoryMovement { get; set; }

    public int IdProduct { get; set; }

    [ForeignKey("IdProduct")]
    public Product? Product { get; set; }

    public int? IdLot { get; set; }

    [ForeignKey("IdLot")]
    public Lot? Lot { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }

    /// <summary>Precio o costo unitario histórico al momento de registrar el movimiento.</summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 9999999.99, ErrorMessage = "El costo unitario debe ser mayor o igual a 0")]
    public decimal UnitCost { get; set; }

    /// <summary>Subtotal de la línea (Cantidad * Costo Unitario).</summary>
    [NotMapped]
    public decimal Subtotal => Quantity * UnitCost;
}
