using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

/// <summary>Datos editables de un usuario. La contraseña es opcional: si llega vacía se conserva la actual.</summary>
public class UpdateUserDto
{
    [Required(ErrorMessage = "El Campo name es requerido")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "El Campo username es requerido")]
    public string? UserName { get; set; }

    /// <summary>Opcional. Si viene con valor se re-encripta con BCrypt.</summary>
    public string? Password { get; set; }

    [Required(ErrorMessage = "El Campo IdRole es requerido")]
    public int IdRole { get; set; }

    public bool IsActive { get; set; } = true;
}
