using System;

namespace Inventory_Flow.Models.Dtos;

public class LotDto
{
    public int IdLot { get; set; }
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime CreationDate { get; set; }

    /// Días restantes hasta la caducidad (negativo si ya venció)
    public int DaysToExpire { get; set; }
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
}
