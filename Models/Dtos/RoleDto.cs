namespace Inventory_Flow.Models.Dtos;

public class RoleDto
{
    public int IdRole { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreateDate { get; set; }
}

