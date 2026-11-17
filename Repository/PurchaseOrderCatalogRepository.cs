using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class PurchaseOrderCatalogRepository : IPurchaseOrderCatalogRepository
{
    private readonly AplicationDbContext _db;

    public PurchaseOrderCatalogRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<PurchaseOrderStatus>> GetStatusesAsync()
    {
        return await _db.PurchaseOrderStatuses
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }

    public async Task<ICollection<PurchaseOrderType>> GetTypesAsync(bool activeOnly = true)
    {
        var query = _db.PurchaseOrderTypes.AsNoTracking().AsQueryable();
        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderBy(t => t.PriorityLevel)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<PurchaseOrderStatus?> GetStatusByIdAsync(int idPurchaseOrderStatus)
    {
        return await _db.PurchaseOrderStatuses.FirstOrDefaultAsync(s => s.IdPurchaseOrderStatus == idPurchaseOrderStatus);
    }

    public async Task<PurchaseOrderStatus?> GetStatusByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var clean = code.Trim().ToUpper();
        return await _db.PurchaseOrderStatuses.FirstOrDefaultAsync(s => s.Code == clean);
    }

    public async Task<PurchaseOrderType?> GetTypeByIdAsync(int idPurchaseOrderType)
    {
        return await _db.PurchaseOrderTypes.FirstOrDefaultAsync(t => t.IdPurchaseOrderType == idPurchaseOrderType);
    }
}
