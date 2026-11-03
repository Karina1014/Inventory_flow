using System;

namespace Inventory_Flow.Models.Dtos;

public class UserLoginResponseDto
{
    public UserRegisterDto? User {get;set;}
    public string? Token {get;set;}
    public string? Message {get;set;}
}
