using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class FinancialApprovalDto
{
    public bool Approved { get; set; }

    [MaxLength(300, ErrorMessage = "La nota o justificación no puede exceder los 300 caracteres")]
    public string? Note { get; set; }
}
