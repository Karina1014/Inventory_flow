using System.Threading.Tasks;
using Inventory_Flow.Models.Dtos.Reports;

namespace Inventory_Flow.Services;

public interface IMatrixStockReportService
{

    Task<byte[]> GenerateMatrixStockPdfAsync(int? idWarehouse = null);
    Task<MatrixStockReportSummaryDto> GetMatrixStockSummaryAsync(int? idWarehouse = null, string? baseUrl = null);
}
