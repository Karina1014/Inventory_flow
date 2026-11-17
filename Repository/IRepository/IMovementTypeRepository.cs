using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IMovementTypeRepository
{
    ICollection<MovementType> GetMovementTypes();
    Task<PagedResultDto<MovementType>> GetMovementTypesPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string? modeFilter = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    MovementType? GetMovementType(int IdMovementType);
    bool IsUniqueCode(string code);
    Task<MovementType> CreateMovementType(CreateMovementTypeDto createMovementTypeDto);
    Task<MovementType> UpdateMovementType(int IdMovementType, UpdateMovementTypeDto updateMovementTypeDto);
    bool DeleteMovementType(int IdMovementType);
}
