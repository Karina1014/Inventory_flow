using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class User
{
    [Key]
    public int IdUser {get;set;}
    public string? Name {get;set;}
    public string Username { get; set; } = string.Empty;
    public string? Password {get;set;}   
    public string? Role {get;set;} 
}
