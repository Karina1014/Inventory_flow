using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Flow.Models;

public class Menu
{
    [Key]
    public int IdMenu { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Order { get; set; }  
    public bool IsActive { get; set; } = true;
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    public string Url { get; set; } = string.Empty;

    public int IdRole { get; set; }

    [ForeignKey("IdRole")]
    public Role? Role { get; set; }
}
