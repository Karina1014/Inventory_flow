using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

/// <summary>
/// DTO para registrar la recepción física de una orden de compra en la bodega central matriz.
/// Asigna lotes del fabricante, fechas de caducidad y crea los movimientos de entrada.
/// </summary>
public class ReceivePurchaseOrderDto
{
    [Required]
    public int IdPurchaseOrder { get; set; }

    [MaxLength(100)]
    public string? DeliveryNoteNumber { get; set; } // Número de guía de remisión o factura física

    [MaxLength(300)]
    public string? ReceptionNotes { get; set; } // Observaciones de inspección sanitaria / calidad

    [Required]
    public List<ReceivePurchaseOrderItemDto> Items { get; set; } = new();
}

public class ReceivePurchaseOrderItemDto
{
    [Required]
    public int IdPurchaseOrderDetail { get; set; }

    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public int OrderedQuantity { get; set; }
    public int PreviouslyReceivedQuantity { get; set; }

    public int RemainingQuantity => Math.Max(0, OrderedQuantity - PreviouslyReceivedQuantity);

    [Range(0, 1000000, ErrorMessage = "La cantidad a recibir debe ser mayor o igual a 0")]
    public int QuantityToReceive { get; set; }

    [MaxLength(50)]
    public string LotNumber { get; set; } = string.Empty;

    public DateTime ExpirationDate { get; set; } = DateTime.Today.AddMonths(12);
}
