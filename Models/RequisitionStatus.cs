using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class RequisitionStatus
{
    [Key]
    public int IdRequisitionStatus { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [MaxLength(20)]
    public string BadgeColor { get; set; } = "secondary";

    public int SortOrder { get; set; }

    public bool IsFinal { get; set; }
}
