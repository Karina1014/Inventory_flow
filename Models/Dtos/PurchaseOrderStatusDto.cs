namespace Inventory_Flow.Models.Dtos;

public class PurchaseOrderStatusDto
{
    public int IdPurchaseOrderStatus { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BadgeColor { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsFinal { get; set; }
}
