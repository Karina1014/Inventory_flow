namespace Inventory_Flow.Models.Dtos;

public class PurchaseOrderTypeDto
{
    public int IdPurchaseOrderType { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PriorityLevel { get; set; }
    public bool IsActive { get; set; }
}
