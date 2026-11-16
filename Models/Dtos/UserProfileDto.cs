namespace Inventory_Flow.Models.Dtos;

public class UserProfileDto
{
    public int IdUser { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;

    public int? IdWarehouse { get; set; }
    public string? WarehouseName { get; set; }
    public string? WarehouseAddress { get; set; }
}

