using System.Threading.Tasks;
using Inventory_Flow.Models;

namespace Inventory_Flow.Services;

public interface IN8nNotificationService
{
    Task<N8nNotification> SendTextMessageAsync(string recipient, string message, int idWarehouse = 3, int idUser = 1);
    Task<N8nNotification> SendStockAlertPdfAsync(string recipient, string message, int idWarehouse, int idUser = 1);
    Task<N8nNotification> SendLowStockSaleAlertAsync(
        int warehouseId,
        string warehouseName,
        int productId,
        string productCode,
        string productName,
        int remainingStock,
        int threshold,
        string recipient,
        int idUser = 1);
}
