using System;

namespace Inventory_Flow.Models.Dtos;

public class ProductDto
{
    public int IdProduct { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int IdCategory { get; set; }
    public DateTime CreationDate { get; set; }
}
