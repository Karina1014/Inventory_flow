using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "La contraseña actual es requerida.")]
    public string? CurrentPassword { get; set; }

    [Required(ErrorMessage = "La nueva contraseña es requerida.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "La contraseña debe incluir al menos una mayúscula, una minúscula, un número y un caracter especial.")]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
    [Compare("NewPassword", ErrorMessage = "La confirmación no coincide con la nueva contraseña.")]
    public string? ConfirmPassword { get; set; }
}
