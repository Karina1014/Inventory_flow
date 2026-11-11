using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IInventoryMovementRepository
{
    ICollection<InventoryMovement> GetInventoryMovements();
    InventoryMovement? GetInventoryMovement(int IdInventoryMovement);
    bool IsUniqueCode(string code);
    Task<InventoryMovement> CreateInventoryMovement(CreateInventoryMovementDto createInventoryMovementDto);
    Task<InventoryMovement> UpdateInventoryMovement(int IdInventoryMovement, UpdateInventoryMovementDto updateInventoryMovementDto);
    bool DeleteInventoryMovement(int IdInventoryMovement);
}
