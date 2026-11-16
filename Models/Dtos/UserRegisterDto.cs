using System;

namespace Inventory_Flow.Models.Dtos;

public class UserRegisterDto
{
    public string? IdUserRegister {get; set;}
    public required string UserName {get; set;}
    public required string Password {get; set;}
    public string? Name {get; set;}
    public string? Role {get; set;}
    public bool IsActive{get;set;}

    public int IdUser { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? IdWarehouse { get; set; }
    public string? WarehouseName { get; set; }
}
