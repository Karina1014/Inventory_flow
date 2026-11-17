using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class CreateSupplierDto
{
    [Required(ErrorMessage = "El RUC o Identificación es obligatorio")]
    [MaxLength(20, ErrorMessage = "El RUC no puede superar los 20 caracteres")]
    public string Ruc { get; set; } = string.Empty;

    [Required(ErrorMessage = "La Razón Social / Nombre Comercial es obligatorio")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "El nombre del contacto no puede superar 100 caracteres")]
    public string ContactName { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
    [MaxLength(100, ErrorMessage = "El correo no puede superar 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "La dirección no puede superar 200 caracteres")]
    public string Address { get; set; } = string.Empty;

    [Range(0, 90, ErrorMessage = "El tiempo de entrega debe ser entre 0 y 90 días")]
    public int DeliveryLeadTimeDays { get; set; } = 1;

    public bool IsActive { get; set; } = true;
}
