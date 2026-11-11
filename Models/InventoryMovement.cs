using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class InventoryMovement
{
    [Key]
    public int IdInventoryMovement {get; set;}
    public string Code { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; } = DateTime.Now;
}
