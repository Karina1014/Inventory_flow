using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Flow.Models.Dtos;

public class UpdateLotDto
{
    [Required(ErrorMessage = "El Campo IdProduct es requerido")]
    public int IdProduct { get; set; }

    [Required(ErrorMessage = "El Campo IdWarehouse es requerido")]
    public int IdWarehouse { get; set; }

    public string LotNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Campo Quantity es requerido")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "El Campo ExpirationDate es requerido")]
    public DateTime ExpirationDate { get; set; }
}
