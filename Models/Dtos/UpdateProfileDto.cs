using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [MaxLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
    public string? Email { get; set; }

    [RegularExpression(@"^\+[1-9]\d{8,14}$", ErrorMessage = "El número debe incluir el prefijo internacional (ej: +593963124396).")]
    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
    public string? Phone { get; set; }
}
