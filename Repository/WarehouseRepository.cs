using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly AplicationDbContext _db;

    public WarehouseRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public ICollection<Warehouse> GetWarehouses()
    {
        return _db.Warehouses.OrderBy(w => w.Name).ToList();
    }

    public Warehouse? GetWarehouse(int IdWarehouse)
    {
        return _db.Warehouses.FirstOrDefault(w => w.IdWarehouse == IdWarehouse);
    }

    public bool IsUniqueName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return !_db.Warehouses.Any(w => w.Name != null && w.Name.ToLower() == name.Trim().ToLower());
    }

    public async Task<Warehouse> CreateWarehouse(CreateWarehouseDto createWarehouseDto)
    {
        var warehouse = new Warehouse()
        {
            Name = createWarehouseDto.Name,
            Address = createWarehouseDto.Address,
            IsActive = true,
            CreationDate = DateTime.Now
        };

        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();
        return warehouse;
    }

    public async Task<Warehouse> UpdateWarehouse(int IdWarehouse, CreateWarehouseDto updateWarehouseDto)
    {
        var warehouse = await _db.Warehouses.FindAsync(IdWarehouse)
            ?? throw new KeyNotFoundException($"Almacén con Id {IdWarehouse} no encontrado.");

        warehouse.Name = updateWarehouseDto.Name;
        warehouse.Address = updateWarehouseDto.Address;

        _db.Warehouses.Update(warehouse);
        await _db.SaveChangesAsync();
        return warehouse;
    }

    public bool DeleteWarehouse(int IdWarehouse)
    {
        var warehouse = _db.Warehouses.FirstOrDefault(w => w.IdWarehouse == IdWarehouse);
        if (warehouse == null) return false;

        _db.Warehouses.Remove(warehouse);
        _db.SaveChanges();
        return true;
    }
}
