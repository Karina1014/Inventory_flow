using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Flow.Models;

public class N8nNotificationType
{
    [Key]
    public int IdN8nNotificationType { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public int IdUserCreated { get; set; } = 1;

    [ForeignKey(nameof(IdUserCreated))]
    public User? UserCreated { get; set; }

    public int IdUserUpdated { get; set; } = 1;

    [ForeignKey(nameof(IdUserUpdated))]
    public User? UserUpdated { get; set; }
}
