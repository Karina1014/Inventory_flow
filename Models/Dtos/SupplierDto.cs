using System;

namespace Inventory_Flow.Models.Dtos;

public class SupplierDto
{
    public int IdSupplier { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int DeliveryLeadTimeDays { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreationDate { get; set; }
}
