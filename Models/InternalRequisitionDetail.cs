using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Flow.Models;

public class InternalRequisitionDetail
{
    [Key]
    public int IdInternalRequisitionDetail { get; set; }

    public int IdInternalRequisition { get; set; }

    [ForeignKey("IdInternalRequisition")]
    public InternalRequisition? InternalRequisition { get; set; }

    public int IdProduct { get; set; }

    [ForeignKey("IdProduct")]
    public Product? Product { get; set; }

    [Required]
    [Range(1, 100000, ErrorMessage = "La cantidad solicitada debe ser mayor a 0")]
    public int RequestedQuantity { get; set; }

    public int DispatchedQuantity { get; set; } = 0;

    [MaxLength(200)]
    public string Note { get; set; } = string.Empty;
}
