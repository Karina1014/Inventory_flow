using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;

namespace Inventory_Flow.Repository;

public class LotRepository : ILotRepository
{
    private readonly AplicationDbContext _db;

    public LotRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public ICollection<Lot> GetLots()
    {
        return _db.Lots.OrderBy(l => l.ExpirationDate).ToList();
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
