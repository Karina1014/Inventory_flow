using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateUserDto
{
    [Required(ErrorMessage = "El Campo username es requerido")]
    public string? UserName {get; set;}

    [Required(ErrorMessage = "El Campo name es requerido")]
    public string? Name {get; set;}

    [Required(ErrorMessage = "El Campo Password es requerido")]
    public string? Password {get; set;}
    [Required(ErrorMessage = "El Campo rol es requerido")]
    public string? Role {get; set;}
}
