using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IMovementTypeRepository
{
    ICollection<MovementType> GetMovementTypes();
    MovementType? GetMovementType(int IdMovementType);
    bool IsUniqueCode(string code);
    Task<MovementType> CreateMovementType(CreateMovementTypeDto createMovementTypeDto);
    Task<MovementType> UpdateMovementType(int IdMovementType, UpdateMovementTypeDto updateMovementTypeDto);
    bool DeleteMovementType(int IdMovementType);
}
