using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class Category
{
    [Key]
    public int IdCategory { get; set; }
    
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; }= string.Empty;
    
    public DateTime CreationDate { get; set; } = DateTime.Now;
    
   
}
