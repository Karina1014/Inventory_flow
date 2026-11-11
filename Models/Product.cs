using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Flow.Models;

public class Product
{
    [Key]
    public int IdProduct { get; set; }

    [Required(ErrorMessage = "El código del producto es requerido")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del producto es requerido")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 9999999.99, ErrorMessage = "El precio sugerido debe ser mayor o igual a 0")]
    public decimal SuggestedPrice { get; set; }

    public bool Iva { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public int IdCategory { get; set; }

    [ForeignKey("IdCategory")]
    public Category? Category { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.Now;
}
