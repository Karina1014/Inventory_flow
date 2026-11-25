using System.Threading.Tasks;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos.Reports;

namespace Inventory_Flow.Services;

public interface IExpirationAlertService
{
    Task<ConsolidatedExpiringLotsReportDto> GetExpiringLotsReportAsync(int daysAhead = 30);
    Task<N8nNotification> DispatchExpiringLotsWhatsAppAlertsAsync(int daysAhead = 30, string? matrizRecipient = null, int idUser = 1);
}
