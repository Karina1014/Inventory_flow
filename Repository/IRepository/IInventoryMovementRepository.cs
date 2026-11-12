using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IInventoryMovementRepository
{
    Task<ICollection<InventoryMovementDto>> GetMovementsAsync();
    Task<InventoryMovementDto?> GetMovementByIdAsync(int id);
    Task<InventoryMovement> CreateMovementAsync(CreateInventoryMovementDto dto);
    Task<InventoryMovement> UpdateMovementAsync(int id, UpdateInventoryMovementDto dto);
    Task<bool> DeleteMovementAsync(int id);
}

