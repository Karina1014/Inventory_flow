using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;

namespace Inventory_Flow.Repository.IRepository;

public interface IPurchaseOrderCatalogRepository
{
    Task<ICollection<PurchaseOrderStatus>> GetStatusesAsync();
    Task<ICollection<PurchaseOrderType>> GetTypesAsync(bool activeOnly = true);
    Task<PurchaseOrderStatus?> GetStatusByIdAsync(int idPurchaseOrderStatus);
    Task<PurchaseOrderStatus?> GetStatusByCodeAsync(string code);
    Task<PurchaseOrderType?> GetTypeByIdAsync(int idPurchaseOrderType);
}
