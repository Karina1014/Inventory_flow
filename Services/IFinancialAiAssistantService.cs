using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos.Ai;

namespace Inventory_Flow.Services;

public interface IFinancialAiAssistantService
{
    Task<List<PurchaseOrder>> GetPendingOrdersForAnalysisAsync();
    Task<FinancialOrderAnalysisDto> AnalyzePurchaseOrderAsync(int orderId);
    Task<GlobalInventoryAiAnalysisDto> AnalyzeGlobalInventoryAsync(int? warehouseId, System.DateTime startDate, System.DateTime endDate);
}
