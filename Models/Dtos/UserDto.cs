using System;

namespace Inventory_Flow.Models.Dtos;

public class UserDto
{

    public int IdUser {get;set;}    
    public string? Name {get;set;}
    public string? UserName {get;set;}
    public string? Role {get;set;} 
    public bool IsActive{get;set;}

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? IdWarehouse { get; set; }
    public string? WarehouseName { get; set; }
    public bool IsAdmin { get; set; }
}
