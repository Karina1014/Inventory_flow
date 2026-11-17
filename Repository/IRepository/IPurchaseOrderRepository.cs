using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IPurchaseOrderRepository
{
    Task<PagedResultDto<PurchaseOrder>> GetOrdersPagedAsync(
        int pageNumber,
        int pageSize,
        int? supplierId = null,
        int? statusId = null,
        int? orderTypeId = null,
        string? search = null,
        bool pendingFinancialOnly = false);

    Task<PurchaseOrder?> GetOrderByIdAsync(int id);
    Task<string> GenerateNextOrderNumberAsync();
    Task<PurchaseOrder> CreateOrderAsync(CreatePurchaseOrderDto dto, int userId);
    Task<bool> ProcessFinancialApprovalAsync(int id, bool approved, int financialUserId, string? note = null);
    Task<bool> UpdateStatusAsync(int id, string newStatusCode, int userId, string? note = null);
    Task<bool> CancelOrderAsync(int id, int userId, string reason);
}
