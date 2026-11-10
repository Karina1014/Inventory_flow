using System;

namespace Inventory_Flow.Models.Dtos;

public class WarehouseDto
{
    public int IdWarehouse { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreationDate { get; set; }
}

