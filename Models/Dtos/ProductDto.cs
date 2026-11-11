using System;

namespace Inventory_Flow.Models.Dtos;

public class ProductDto
{
    public int IdProduct { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal SuggestedPrice { get; set; }
    public bool Iva { get; set; }
    public bool IsActive { get; set; }
    public int IdCategory { get; set; }
    public DateTime CreationDate { get; set; }
}
