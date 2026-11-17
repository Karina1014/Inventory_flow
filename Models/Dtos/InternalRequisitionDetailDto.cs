namespace Inventory_Flow.Models.Dtos;

public class InternalRequisitionDetailDto
{
    public int IdInternalRequisitionDetail { get; set; }
    public int IdInternalRequisition { get; set; }
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int DispatchedQuantity { get; set; }
    public string Note { get; set; } = string.Empty;
}
