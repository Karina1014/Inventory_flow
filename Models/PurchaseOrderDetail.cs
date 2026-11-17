using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Flow.Models;

public class PurchaseOrderDetail
{
    [Key]
    public int IdPurchaseOrderDetail { get; set; }

    public int IdPurchaseOrder { get; set; }

    [ForeignKey("IdPurchaseOrder")]
    public PurchaseOrder? PurchaseOrder { get; set; }

    public int IdProduct { get; set; }

    [ForeignKey("IdProduct")]
    public Product? Product { get; set; }

    [Required]
    [Range(1, 1000000, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }

    public int ReceivedQuantity { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.00, 9999999.99, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxPercentage { get; set; } = 15.00m; // Tarifa IVA estándar Ecuador

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(200)]
    public string Notes { get; set; } = string.Empty;

    public int? IdInternalRequisition { get; set; }

    [ForeignKey("IdInternalRequisition")]
    public InternalRequisition? InternalRequisition { get; set; }
}
