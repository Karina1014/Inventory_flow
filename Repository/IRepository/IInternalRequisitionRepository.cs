using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IInternalRequisitionRepository
{
    Task<PagedResultDto<InternalRequisition>> GetRequisitionsPagedAsync(
        int pageNumber,
        int pageSize,
        int? requestingWarehouseId = null,
        string? statusCode = null,
        int? requisitionTypeId = null,
        string? search = null,
        bool prioritizeByType = false);

    Task<InternalRequisition?> GetRequisitionByIdAsync(int id);
    Task<string> GenerateNextRequisitionCodeAsync();

    /// <exception cref="System.InvalidOperationException">Si el tipo no existe o está inactivo.</exception>
    Task<InternalRequisition> CreateRequisitionAsync(CreateInternalRequisitionDto dto, int userId);

    /// <exception cref="System.InvalidOperationException">Si la transición de estado no está permitida.</exception>
    Task<bool> UpdateStatusAsync(int id, string newStatusCode, int reviewedByUserId, string? reviewNote = null);

    Task<bool> UpdateDispatchedQuantitiesAsync(int id, Dictionary<int, int> productDispatchedQuantities, int reviewedByUserId);
    Task<bool> ConfirmBranchReceiptAsync(int id, int userId, string? confirmationNote = null);
    Task<bool> CancelRequisitionAsync(int id, int userId, string reason);
}
