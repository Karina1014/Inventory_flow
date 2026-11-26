using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory_Flow.Services;

public class RecipientInfo
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int? WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public interface INotificationRecipientService
{
    // Obtiene el teléfono del Bodeguero Principal o Administrador de Matriz para alertas de reposición y resumen de caducidades
    Task<string> GetBodegueroPrincipalPhoneAsync();

    // Obtiene el teléfono del Bodeguero o Recepcionista asignado a una sucursal específica para rotación PEPS
    Task<string> GetBranchRecipientPhoneAsync(int idWarehouse);

    // Obtiene la lista de destinatarios activos con teléfono configurado para trazabilidad y auditoría
    Task<List<RecipientInfo>> GetRecipientsSummaryAsync();
}
