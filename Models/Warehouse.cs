using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class Warehouse
{
    [Key]
    public int IdWarehouse { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Address { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreationDate { get; set; } = DateTime.Now;
}
