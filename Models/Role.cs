using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class Role
{
    [Key]
    public int IdRole {get;set;}
    public string? Name {get;set;} = string.Empty;
    public string? Description {get;set;}
    public DateTime CreateDate { get; set;} = DateTime.UtcNow;

}