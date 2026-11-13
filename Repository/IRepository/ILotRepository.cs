using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface ILotRepository
{
    ICollection<Lot> GetLots();
    ICollection<Lot> GetExpiringLots(int days);
    ICollection<Lot> GetExpiredLots();
    Lot? GetLot(int IdLot);
    Task<Lot> CreateLot(CreateLotDto createLotDto);
    Task<Lot> UpdateLot(int IdLot, UpdateLotDto updateLotDto);
    bool DeleteLot(int IdLot);
}
