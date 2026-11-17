using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models;

/// <summary>
/// Catálogo de tipos de requisición (estrategia / nivel de servicio).
/// Es INMUTABLE en la requisición: se define al crearla y no cambia.
/// </summary>
public class RequisitionType
{
    [Key]
    public int IdRequisitionType { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Prioridad de atención en Carapungo: 1 = más urgente.</summary>
    [Range(1, 10)]
    public int PriorityLevel { get; set; } = 5;

    /// <summary>Tiempo máximo de atención comprometido (SLA) en horas.</summary>
    [Range(1, 720)]
    public int MaxResponseHours { get; set; } = 72;

    public bool IsActive { get; set; } = true;
}
