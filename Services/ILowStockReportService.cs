using System.Threading.Tasks;
using Inventory_Flow.Models.Dtos.Reports;

namespace Inventory_Flow.Services;

public interface ILowStockReportService
{
    Task<byte[]> GenerateLowStockPdfByWarehouseAsync(int idWarehouse);
    Task<LowStockReportSummaryDto> GetLowStockSummaryByWarehouseAsync(int idWarehouse, string? baseUrl = null);
}
