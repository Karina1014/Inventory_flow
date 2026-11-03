using System;

namespace Inventory_Flow.Models.Dtos;

public class UserRegisterDto
{
    public string? IdUserRegister {get; set;}
    public required string UserName {get; set;}
    public required string Password {get; set;}
    public string? Name {get; set;}
    public string? Role {get; set;}
}
