using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Inventory_Flow.Models;

public class InventoryMovement : IValidatableObject
{
    [Key]
    public int IdInventoryMovement { get; set; }

    /// <summary>Fecha del movimiento. La asigna el servidor al registrar.</summary>
    public DateTime MovementDate { get; set; } = DateTime.Now;

    public int IdMovementType { get; set; }

    [ForeignKey("IdMovementType")]
    public MovementType? MovementType { get; set; }

    /// <summary>Almacén de salida. Nulo en una entrada pura.</summary>
    public int? IdOriginWarehouse { get; set; }

    [ForeignKey("IdOriginWarehouse")]
    public Warehouse? OriginWarehouse { get; set; }

    /// <summary>Almacén de entrada. Nulo en una salida pura.</summary>
    public int? IdDestinationWarehouse { get; set; }

    [ForeignKey("IdDestinationWarehouse")]
    public Warehouse? DestinationWarehouse { get; set; }

    /// <summary>FK al usuario que registró el movimiento (trazabilidad).</summary>
    public int? IdUser { get; set; }

    [ForeignKey("IdUser")]
    public User? User { get; set; }

    [MaxLength(255)]
    public string Observation { get; set; } = string.Empty;

    /// <summary>ID del documento que originó el movimiento (ej. IdVenta, IdCompra).</summary>
    public int? ReferenceId { get; set; }

    /// <summary>Origen de la transacción: "VENTA", "COMPRA", "AJUSTE", "MANUAL", etc.</summary>
    [MaxLength(50)]
    public string? ReferenceSource { get; set; }

    public List<InventoryMovementDetail> Details { get; set; } = new();

    /// <summary>Total de unidades del movimiento (no se persiste).</summary>
    [NotMapped]
    public int TotalQuantity => Details?.Sum(d => d.Quantity) ?? 0;

    /// <summary>Valor total del movimiento a costo histórico (no se persiste).</summary>
    [NotMapped]
    public decimal TotalCost => Details?.Sum(d => d.Subtotal) ?? 0;

    /// <summary>
    /// Reglas de negocio: el sentido del movimiento se deduce de los almacenes y el tipo.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IdMovementType <= 0)
        {
            yield return new ValidationResult(
                "Debe seleccionar un tipo de movimiento.",
                new[] { nameof(IdMovementType) });
        }

        if (IdOriginWarehouse is null && IdDestinationWarehouse is null)
        {
            yield return new ValidationResult(
                "Debe indicar al menos un almacén (origen o destino).",
                new[] { nameof(IdOriginWarehouse), nameof(IdDestinationWarehouse) });
        }

        if (IdOriginWarehouse is not null && IdOriginWarehouse == IdDestinationWarehouse)
        {
            yield return new ValidationResult(
                "El almacén de origen y el de destino no pueden ser el mismo.",
                new[] { nameof(IdDestinationWarehouse) });
        }

        if (Details is null || Details.Count == 0)
        {
            yield return new ValidationResult(
                "Debe registrar al menos un producto en el movimiento.",
                new[] { nameof(Details) });
            yield break;
        }

        if (Details.Any(d => d.Quantity <= 0))
        {
            yield return new ValidationResult(
                "Todas las cantidades del detalle deben ser mayores a 0.",
                new[] { nameof(Details) });
        }

        if (Details.Any(d => d.UnitCost < 0))
        {
            yield return new ValidationResult(
                "El costo unitario no puede ser negativo.",
                new[] { nameof(Details) });
        }

        if (Details.Any(d => d.IdProduct <= 0))
        {
            yield return new ValidationResult(
                "Cada línea del detalle debe tener un producto válido.",
                new[] { nameof(Details) });
        }

        if (Details.GroupBy(d => d.IdProduct).Any(g => g.Count() > 1))
        {
            yield return new ValidationResult(
                "No se puede repetir el mismo producto en el detalle del movimiento.",
                new[] { nameof(Details) });
        }
    }
}
