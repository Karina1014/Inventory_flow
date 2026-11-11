using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;

namespace Inventory_Flow.Repository;

public class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly AplicationDbContext _db;

    public InventoryMovementRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public ICollection<InventoryMovement> GetInventoryMovements()
    {
        return _db.InventoryMovements.OrderByDescending(m => m.CreationDate).ToList();
    }

    public InventoryMovement? GetInventoryMovement(int IdInventoryMovement)
    {
        return _db.InventoryMovements.FirstOrDefault(m => m.IdInventoryMovement == IdInventoryMovement);
    }

    public bool IsUniqueCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        return !_db.InventoryMovements.Any(m => m.Code != null && m.Code.ToLower() == code.Trim().ToLower());
    }

    public async Task<InventoryMovement> CreateInventoryMovement(CreateInventoryMovementDto createInventoryMovementDto)
    {
        var movement = new InventoryMovement()
        {
            Code = createInventoryMovementDto.Code,
            Mode = createInventoryMovementDto.Mode,
            Description = createInventoryMovementDto.Description,
            CreationDate = DateTime.Now
        };

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync();
        return movement;
    }

    public async Task<InventoryMovement> UpdateInventoryMovement(int IdInventoryMovement, UpdateInventoryMovementDto updateInventoryMovementDto)
    {
        var movement = await _db.InventoryMovements.FindAsync(IdInventoryMovement)
            ?? throw new KeyNotFoundException($"Movimiento de inventario con Id {IdInventoryMovement} no encontrado.");

        movement.Code = updateInventoryMovementDto.Code;
        movement.Mode = updateInventoryMovementDto.Mode;
        movement.Description = updateInventoryMovementDto.Description;

        _db.InventoryMovements.Update(movement);
        await _db.SaveChangesAsync();
        return movement;
    }

    public bool DeleteInventoryMovement(int IdInventoryMovement)
    {
        var movement = _db.InventoryMovements.FirstOrDefault(m => m.IdInventoryMovement == IdInventoryMovement);
        if (movement == null) return false;

        _db.InventoryMovements.Remove(movement);
        _db.SaveChanges();
        return true;
    }
}
