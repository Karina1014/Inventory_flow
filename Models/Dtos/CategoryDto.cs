using System;

namespace Inventory_Flow.Models.Dtos;

public class CategoryDto
{
    public int IdCategory { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}
