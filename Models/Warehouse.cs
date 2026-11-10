using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class Warehouse
{
    [Key]
    public int IdWarehouse {get;set;}
    public string Name {get; set;} = string.Empty;
    public string Addres {get;set;} = string.Empty;
    public bool IsActive {get;set;}
    public DateTime CreationDate { get; set; } = DateTime.Now;
}
