using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Inventory_Flow.Models;

public class InternalRequisition
{
    [Key]
    public int IdInternalRequisition { get; set; }

    [Required]
    [MaxLength(30)]
    public string RequisitionCode { get; set; } = string.Empty;

    public DateTime RequisitionDate { get; set; } = DateTime.Now;

    // Sucursal solicitante (origen del requerimiento)
    public int IdRequestingWarehouse { get; set; }

    [ForeignKey("IdRequestingWarehouse")]
    public Warehouse? RequestingWarehouse { get; set; }

    // Sucursal proveedora (normalmente Matriz Carapungo)
    public int IdFulfillingWarehouse { get; set; }

    [ForeignKey("IdFulfillingWarehouse")]
    public Warehouse? FulfillingWarehouse { get; set; }

    // Usuario que solicita el pedido
    public int IdUser { get; set; }

    [ForeignKey("IdUser")]
    public User? User { get; set; }

    // Tipo de requisición (inmutable): define prioridad y SLA
    public int IdRequisitionType { get; set; }

    [ForeignKey("IdRequisitionType")]
    public RequisitionType? RequisitionType { get; set; }

    // Estado actual del flujo (mutable)
    public int IdRequisitionStatus { get; set; }

    [ForeignKey("IdRequisitionStatus")]
    public RequisitionStatus? RequisitionStatus { get; set; }

    [MaxLength(500)]
    public string Observation { get; set; } = string.Empty;

    public DateTime? ReviewedDate { get; set; }

    public int? IdReviewedByUser { get; set; }

    [ForeignKey("IdReviewedByUser")]
    public User? ReviewedByUser { get; set; }

    [MaxLength(255)]
    public string? ReviewNote { get; set; }

    public List<InternalRequisitionDetail> Details { get; set; } = new();

    [NotMapped]
    public int TotalRequestedQuantity => Details?.Sum(d => d.RequestedQuantity) ?? 0;

    [NotMapped]
    public int TotalDispatchedQuantity => Details?.Sum(d => d.DispatchedQuantity) ?? 0;
}
