using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

public class User
{
    [Key]
    public int IdUser {get;set;}
    [Required]
    public string? Name {get;set;}
    public string? UserName {get;set;}
    public string? Password {get;set;}   
    public string? Role {get;set;} 
}
