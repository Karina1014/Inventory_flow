using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class PurchaseOrderType
{
    [Key]
    public int IdPurchaseOrderType { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int PriorityLevel { get; set; } = 3;

    public bool IsActive { get; set; } = true;
}
