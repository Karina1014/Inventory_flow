using Inventory_Flow.Data;
using Inventory_Flow.Extensions;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class LotRepository : ILotRepository
{
    private readonly AplicationDbContext _db;

    public LotRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public ICollection<Lot> GetLots(int? warehouseId = null)
    {
        var query = _db.Lots
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .AsNoTracking();

        if (warehouseId.HasValue)
        {
            query = query.Where(l => l.IdWarehouse == warehouseId.Value);
        }

        return query
            .OrderByDescending(l => l.CreationDate)
            .ThenByDescending(l => l.IdLot)
            .ToList();
    }

    public async Task<PagedResultDto<Lot>> GetLotsPagedAsync(int pageNumber, int pageSize, string filter = "all", int alertDays = 30, int? warehouseId = null, string? search = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var today = DateTime.Today;
        var limit = today.AddDays(alertDays + 1);

        var query = _db.Lots
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(l =>
                (l.LotNumber != null && l.LotNumber.Contains(term)) ||
                (l.Product != null && l.Product.Name != null && l.Product.Name.Contains(term)) ||
                (l.Product != null && l.Product.Code != null && l.Product.Code.Contains(term)) ||
                (l.Warehouse != null && l.Warehouse.Name != null && l.Warehouse.Name.Contains(term)));
        }

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            query = query.Where(l => l.IdWarehouse == warehouseId.Value);
        }

        if (filter == "expired")
        {
            query = query.Where(l => l.ExpirationDate.Date < today);
        }
        else if (filter == "soon")
        {
            query = query.Where(l => l.ExpirationDate.Date >= today && l.ExpirationDate < limit);
        }
        else if (filter == "valid")
        {
            query = query.Where(l => l.ExpirationDate >= limit);
        }

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(l => l.CreationDate >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(l => l.CreationDate < end);
        }

        return await query
            .OrderByDescending(l => l.CreationDate)
            .ThenByDescending(l => l.IdLot)
            .ToPagedResultAsync(pageNumber, pageSize);
    }

    public ICollection<Lot> GetExpiringLots(int days, int? warehouseId = null)
    {
        var limit = DateTime.Today.AddDays(days + 1);
        var query = _db.Lots
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .AsNoTracking()
            .Where(l => l.Quantity > 0 && l.ExpirationDate < limit);

        if (warehouseId.HasValue)
        {
            query = query.Where(l => l.IdWarehouse == warehouseId.Value);
        }

        return query.OrderBy(l => l.ExpirationDate).ToList();
    }

    public ICollection<Lot> GetExpiredLots(int? warehouseId = null)
    {
        var today = DateTime.Today;
        var query = _db.Lots
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .AsNoTracking()
            .Where(l => l.Quantity > 0 && l.ExpirationDate < today);

        if (warehouseId.HasValue)
        {
            query = query.Where(l => l.IdWarehouse == warehouseId.Value);
        }

        return query.OrderBy(l => l.ExpirationDate).ToList();
    }

    public Lot? GetLot(int IdLot)
    {
        return _db.Lots.FirstOrDefault(l => l.IdLot == IdLot);
    }

    public async Task<Lot> CreateLot(CreateLotDto createLotDto)
    {
        var lot = new Lot()
        {
            IdProduct = createLotDto.IdProduct,
            IdWarehouse = createLotDto.IdWarehouse,
            LotNumber = createLotDto.LotNumber,
            Quantity = createLotDto.Quantity,
            ExpirationDate = createLotDto.ExpirationDate,
            CreationDate = DateTime.Now
        };

        _db.Lots.Add(lot);
        await _db.SaveChangesAsync();
        return lot;
    }

    public async Task<Lot> UpdateLot(int IdLot, UpdateLotDto updateLotDto)
    {
        var lot = await _db.Lots.FindAsync(IdLot)
            ?? throw new KeyNotFoundException($"Lote con Id {IdLot} no encontrado.");

        lot.IdProduct = updateLotDto.IdProduct;
        lot.IdWarehouse = updateLotDto.IdWarehouse;
        lot.LotNumber = updateLotDto.LotNumber;
        lot.Quantity = updateLotDto.Quantity;
        lot.ExpirationDate = updateLotDto.ExpirationDate;

        _db.Lots.Update(lot);
        await _db.SaveChangesAsync();
        return lot;
    }

    public bool DeleteLot(int IdLot)
    {
        var lot = _db.Lots.FirstOrDefault(l => l.IdLot == IdLot);
        if (lot == null) return false;

        _db.Lots.Remove(lot);
        _db.SaveChanges();
        return true;
    }
}
