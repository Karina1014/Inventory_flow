using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class RequisitionCatalogRepository : IRequisitionCatalogRepository
{
    private readonly AplicationDbContext _db;

    public RequisitionCatalogRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<RequisitionStatus>> GetStatusesAsync()
    {
        return await _db.RequisitionStatuses
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }

    public async Task<ICollection<RequisitionType>> GetTypesAsync(bool activeOnly = true)
    {
        var query = _db.RequisitionTypes.AsNoTracking().AsQueryable();
        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderBy(t => t.PriorityLevel)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<RequisitionStatus?> GetStatusByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var clean = code.Trim().ToUpper();
        return await _db.RequisitionStatuses.FirstOrDefaultAsync(s => s.Code == clean);
    }

    public async Task<RequisitionType?> GetTypeByIdAsync(int idRequisitionType)
    {
        return await _db.RequisitionTypes.FirstOrDefaultAsync(t => t.IdRequisitionType == idRequisitionType);
    }
}
