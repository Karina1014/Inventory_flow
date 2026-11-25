using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Flow.Models;

public class N8nNotification
{
    [Key]
    public int IdN8nNotification { get; set; }

    [Required]
    public int IdN8nNotificationType { get; set; }

    [ForeignKey(nameof(IdN8nNotificationType))]
    public N8nNotificationType? NotificationType { get; set; }

    public int IdWarehouse { get; set; } = 3;

    [ForeignKey(nameof(IdWarehouse))]
    public Warehouse? Warehouse { get; set; }

    public int IdUser { get; set; } = 1;

    [ForeignKey(nameof(IdUser))]
    public User? User { get; set; }

    [Required]
    [MaxLength(150)]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PdfPath { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [Required]
    public string Response { get; set; } = string.Empty;

    public DateTime SentDate { get; set; } = DateTime.Now;
}
