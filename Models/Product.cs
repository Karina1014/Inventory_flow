using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Flow.Models;

public class Product
{
    [Key]
    public int IdProduct { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int IdCategory { get; set; }

    [ForeignKey("IdCategory")]
    public Category? Category { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.Now;
}
