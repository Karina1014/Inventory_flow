using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class DispatchRequisitionDto
{
    [Required]
    public Dictionary<int, int> ProductDispatchedQuantities { get; set; } = new();
}

public class ConfirmReceiptRequisitionDto
{
    [MaxLength(255)]
    public string? ConfirmationNote { get; set; }
}
