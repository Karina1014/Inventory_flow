using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;

namespace Inventory_Flow.Repository;

public class MovementTypeRepository : IMovementTypeRepository
{
    private readonly AplicationDbContext _db;

    public MovementTypeRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public ICollection<MovementType> GetMovementTypes()
    {
        return _db.MovementTypes.OrderByDescending(m => m.CreationDate).ToList();
    }

    public MovementType? GetMovementType(int IdMovementType)
    {
        return _db.MovementTypes.FirstOrDefault(m => m.IdMovementType == IdMovementType);
    }

    public bool IsUniqueCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        return !_db.MovementTypes.Any(m => m.Code != null && m.Code.ToLower() == code.Trim().ToLower());
    }

    public async Task<MovementType> CreateMovementType(CreateMovementTypeDto createMovementTypeDto)
    {
        var movement = new MovementType()
        {
            Code = createMovementTypeDto.Code,
            Mode = createMovementTypeDto.Mode,
            Description = createMovementTypeDto.Description,
            CreationDate = DateTime.Now
        };

        _db.MovementTypes.Add(movement);
        await _db.SaveChangesAsync();
        return movement;
    }

    public async Task<MovementType> UpdateMovementType(int IdMovementType, UpdateMovementTypeDto updateMovementTypeDto)
    {
        var movement = await _db.MovementTypes.FindAsync(IdMovementType)
            ?? throw new KeyNotFoundException($"Movimiento de inventario con Id {IdMovementType} no encontrado.");

        movement.Code = updateMovementTypeDto.Code;
        movement.Mode = updateMovementTypeDto.Mode;
        movement.Description = updateMovementTypeDto.Description;

        _db.MovementTypes.Update(movement);
        await _db.SaveChangesAsync();
        return movement;
    }

    public bool DeleteMovementType(int IdMovementType)
    {
        var movement = _db.MovementTypes.FirstOrDefault(m => m.IdMovementType == IdMovementType);
        if (movement == null) return false;

        _db.MovementTypes.Remove(movement);
        _db.SaveChanges();
        return true;
    }
}
