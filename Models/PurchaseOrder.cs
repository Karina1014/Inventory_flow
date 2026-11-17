using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Inventory_Flow.Models;

public class PurchaseOrder
{
    [Key]
    public int IdPurchaseOrder { get; set; }

    [Required]
    [MaxLength(30)]
    public string OrderNumber { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public int IdSupplier { get; set; }

    [ForeignKey("IdSupplier")]
    public Supplier? Supplier { get; set; }

    public int IdDestinationWarehouse { get; set; }

    [ForeignKey("IdDestinationWarehouse")]
    public Warehouse? DestinationWarehouse { get; set; }

    public int IdUser { get; set; }

    [ForeignKey("IdUser")]
    public User? User { get; set; }

    public int IdPurchaseOrderType { get; set; }

    [ForeignKey("IdPurchaseOrderType")]
    public PurchaseOrderType? PurchaseOrderType { get; set; }

    public int IdPurchaseOrderStatus { get; set; }

    [ForeignKey("IdPurchaseOrderStatus")]
    public PurchaseOrderStatus? PurchaseOrderStatus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(500)]
    public string Observation { get; set; } = string.Empty;

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? FinancialApprovalDate { get; set; }

    public int? IdFinancialApprovalUser { get; set; }

    [ForeignKey("IdFinancialApprovalUser")]
    public User? FinancialApprovalUser { get; set; }

    [MaxLength(300)]
    public string? FinancialApprovalNote { get; set; }

    public List<PurchaseOrderDetail> Details { get; set; } = new();

    [NotMapped]
    public int TotalItemsCount => Details?.Sum(d => d.Quantity) ?? 0;
}
