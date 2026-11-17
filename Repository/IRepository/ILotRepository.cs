using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface ILotRepository
{
    ICollection<Lot> GetLots(int? warehouseId = null);
    Task<PagedResultDto<Lot>> GetLotsPagedAsync(int pageNumber, int pageSize, string filter = "all", int alertDays = 30, int? warehouseId = null);
    ICollection<Lot> GetExpiringLots(int days, int? warehouseId = null);
    ICollection<Lot> GetExpiredLots(int? warehouseId = null);
    Lot? GetLot(int IdLot);
    Task<Lot> CreateLot(CreateLotDto createLotDto);
    Task<Lot> UpdateLot(int IdLot, UpdateLotDto updateLotDto);
    bool DeleteLot(int IdLot);
}
