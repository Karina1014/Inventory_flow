using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IWarehouseRepository
{
    ICollection<Warehouse> GetWarehouses();
    Warehouse? GetWarehouse(int IdWarehouse);
    bool IsUniqueName(string name);
    Task<Warehouse> CreateWarehouse(CreateWarehouseDto createWarehouseDto);
    Task<Warehouse> UpdateWarehouse(int IdWarehouse, CreateWarehouseDto updateWarehouseDto);
    bool DeleteWarehouse(int IdWarehouse);
}
