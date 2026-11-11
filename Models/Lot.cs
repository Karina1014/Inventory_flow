using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Flow.Models;

public class Lot
{
    [Key]
    public int IdLot { get; set; }

    public int IdProduct { get; set; }

    [ForeignKey("IdProduct")]
    public Product? Product { get; set; }

    public int IdWarehouse { get; set; }

    [ForeignKey("IdWarehouse")]
    public Warehouse? Warehouse { get; set; }

    public string LotNumber { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime ExpirationDate { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.Now;
}
