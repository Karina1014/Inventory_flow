namespace Inventory_Flow.Models.Dtos;

public class MenuDto
{
    public int IdMenu { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Order { get; set; }
    public int IdRole { get; set; }
    public string? RoleName { get; set; }
}
