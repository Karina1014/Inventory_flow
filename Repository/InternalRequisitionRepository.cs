using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Extensions;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class InternalRequisitionRepository : IInternalRequisitionRepository
{
    private readonly AplicationDbContext _db;
    private readonly IRequisitionCatalogRepository _catalog;

    public InternalRequisitionRepository(AplicationDbContext db, IRequisitionCatalogRepository catalog)
    {
        _db = db;
        _catalog = catalog;
    }

    private IQueryable<InternalRequisition> QueryWithIncludes()
    {
        return _db.InternalRequisitions
            .Include(r => r.RequestingWarehouse)
            .Include(r => r.FulfillingWarehouse)
            .Include(r => r.User)
            .Include(r => r.ReviewedByUser)
            .Include(r => r.RequisitionType)
            .Include(r => r.RequisitionStatus)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product!)
                    .ThenInclude(p => p.Category);
    }

    public async Task<PagedResultDto<InternalRequisition>> GetRequisitionsPagedAsync(
        int pageNumber,
        int pageSize,
        int? requestingWarehouseId = null,
        string? statusCode = null,
        int? requisitionTypeId = null,
        string? search = null,
        bool prioritizeByType = false)
    {
        var query = QueryWithIncludes().AsNoTracking().AsSplitQuery();

        if (requestingWarehouseId.HasValue && requestingWarehouseId.Value > 0)
        {
            query = query.Where(r => r.IdRequestingWarehouse == requestingWarehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusCode) && !string.Equals(statusCode, "all", StringComparison.OrdinalIgnoreCase))
        {
            var code = statusCode.Trim().ToUpper();
            query = query.Where(r => r.RequisitionStatus != null && r.RequisitionStatus.Code == code);
        }

        if (requisitionTypeId.HasValue && requisitionTypeId.Value > 0)
        {
            query = query.Where(r => r.IdRequisitionType == requisitionTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(r =>
                r.RequisitionCode.ToLower().Contains(term) ||
                (r.RequestingWarehouse != null && r.RequestingWarehouse.Name.ToLower().Contains(term)) ||
                r.Observation.ToLower().Contains(term));
        }

        IOrderedQueryable<InternalRequisition> ordered = prioritizeByType
            // Bandeja Carapungo: primero lo más urgente y, dentro de cada prioridad, lo más antiguo (FIFO)
            ? query.OrderBy(r => r.RequisitionType!.PriorityLevel).ThenBy(r => r.RequisitionDate)
            // Historial: lo más reciente primero
            : query.OrderByDescending(r => r.RequisitionDate).ThenByDescending(r => r.IdInternalRequisition);

        return await ordered.ToPagedResultAsync(pageNumber, pageSize);
    }

    public async Task<InternalRequisition?> GetRequisitionByIdAsync(int id)
    {
        return await QueryWithIncludes()
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.IdInternalRequisition == id);
    }

    public async Task<string> GenerateNextRequisitionCodeAsync()
    {
        var prefix = $"REQ-{DateTime.Now.Year}-";

        var lastCode = await _db.InternalRequisitions
            .Where(r => r.RequisitionCode.StartsWith(prefix))
            .OrderByDescending(r => r.IdInternalRequisition)
            .Select(r => r.RequisitionCode)
            .FirstOrDefaultAsync();

        var nextSeq = 1;
        if (!string.IsNullOrEmpty(lastCode))
        {
            var parts = lastCode.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var currentSeq))
            {
                nextSeq = currentSeq + 1;
            }
        }

        return $"{prefix}{nextSeq:D4}";
    }

    public async Task<InternalRequisition> CreateRequisitionAsync(CreateInternalRequisitionDto dto, int userId)
    {
        var type = await _catalog.GetTypeByIdAsync(dto.IdRequisitionType);
        if (type == null || !type.IsActive)
        {
            throw new InvalidOperationException("El tipo de requisición seleccionado no existe o está inactivo.");
        }

        var initialStatus = await _catalog.GetStatusByCodeAsync(RequisitionStatusCodes.Solicitado)
            ?? throw new InvalidOperationException("No está configurado el estado inicial SOLICITADO en el catálogo.");

        // Si no se especifica la bodega proveedora, se usa la matriz (Carapungo)
        var fulfillingWarehouseId = dto.IdFulfillingWarehouse;
        if (!fulfillingWarehouseId.HasValue || fulfillingWarehouseId.Value <= 0)
        {
            var matriz = await _db.Warehouses
                .Where(w => w.IsActive)
                .FirstOrDefaultAsync(w => w.Name.Contains("Carapungo") || w.Name.Contains("Matriz") || w.Name.Contains("Central"));

            fulfillingWarehouseId = matriz?.IdWarehouse
                ?? throw new InvalidOperationException("No se encontró la bodega matriz (Carapungo) para atender la requisición.");
        }

        if (fulfillingWarehouseId.Value == dto.IdRequestingWarehouse)
        {
            throw new InvalidOperationException("La sucursal solicitante no puede ser la misma que la bodega que atiende el pedido.");
        }

        var requisition = new InternalRequisition
        {
            RequisitionCode = await GenerateNextRequisitionCodeAsync(),
            RequisitionDate = DateTime.Now,
            IdRequestingWarehouse = dto.IdRequestingWarehouse,
            IdFulfillingWarehouse = fulfillingWarehouseId.Value,
            IdUser = userId,
            IdRequisitionType = type.IdRequisitionType,
            IdRequisitionStatus = initialStatus.IdRequisitionStatus,
            Observation = dto.Observation?.Trim() ?? string.Empty
        };

        // Consolidar productos repetidos en una sola línea
        foreach (var group in dto.Details.Where(d => d.RequestedQuantity > 0).GroupBy(d => d.IdProduct))
        {
            requisition.Details.Add(new InternalRequisitionDetail
            {
                IdProduct = group.Key,
                RequestedQuantity = group.Sum(d => d.RequestedQuantity),
                DispatchedQuantity = 0,
                Note = string.Join(" | ", group.Select(d => d.Note?.Trim()).Where(n => !string.IsNullOrEmpty(n)))
            });
        }

        if (requisition.Details.Count == 0)
        {
            throw new InvalidOperationException("La requisición debe tener al menos un producto con cantidad mayor a 0.");
        }

        _db.InternalRequisitions.Add(requisition);
        await _db.SaveChangesAsync();

        return requisition;
    }

    public async Task<bool> UpdateStatusAsync(int id, string newStatusCode, int reviewedByUserId, string? reviewNote = null)
    {
        var requisition = await _db.InternalRequisitions
            .Include(r => r.RequisitionStatus)
            .FirstOrDefaultAsync(r => r.IdInternalRequisition == id);

        if (requisition == null) return false;

        var newStatus = await _catalog.GetStatusByCodeAsync(newStatusCode)
            ?? throw new InvalidOperationException($"El estado '{newStatusCode}' no existe en el catálogo.");

        var currentCode = requisition.RequisitionStatus?.Code ?? string.Empty;
        if (!RequisitionStatusCodes.CanTransition(currentCode, newStatus.Code))
        {
            throw new InvalidOperationException(
                $"No se puede cambiar la requisición de '{requisition.RequisitionStatus?.Name}' a '{newStatus.Name}'.");
        }

        requisition.IdRequisitionStatus = newStatus.IdRequisitionStatus;
        requisition.ReviewedDate = DateTime.Now;
        requisition.IdReviewedByUser = reviewedByUserId;
        if (!string.IsNullOrWhiteSpace(reviewNote))
        {
            requisition.ReviewNote = reviewNote.Trim();
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateDispatchedQuantitiesAsync(int id, Dictionary<int, int> productDispatchedQuantities, int reviewedByUserId)
    {
        var requisition = await _db.InternalRequisitions
            .Include(r => r.Details)
            .Include(r => r.RequisitionStatus)
            .FirstOrDefaultAsync(r => r.IdInternalRequisition == id);

        if (requisition == null) return false;

        foreach (var detail in requisition.Details)
        {
            if (productDispatchedQuantities.TryGetValue(detail.IdProduct, out var qty))
            {
                // No se despacha más de lo solicitado ni cantidades negativas
                detail.DispatchedQuantity = Math.Clamp(qty, 0, detail.RequestedQuantity);
            }
        }

        var totalReq = requisition.Details.Sum(d => d.RequestedQuantity);
        var totalDisp = requisition.Details.Sum(d => d.DispatchedQuantity);

        string? targetCode = totalDisp <= 0
            ? null
            : totalDisp >= totalReq ? RequisitionStatusCodes.Despachado : RequisitionStatusCodes.AprobadoParcial;

        if (targetCode != null && !string.Equals(requisition.RequisitionStatus?.Code, targetCode, StringComparison.OrdinalIgnoreCase))
        {
            var currentCode = requisition.RequisitionStatus?.Code ?? string.Empty;
            if (!RequisitionStatusCodes.CanTransition(currentCode, targetCode))
            {
                throw new InvalidOperationException(
                    $"No se puede registrar el despacho cuando la requisición está en estado '{requisition.RequisitionStatus?.Name}'.");
            }

            var targetStatus = await _catalog.GetStatusByCodeAsync(targetCode)
                ?? throw new InvalidOperationException($"El estado '{targetCode}' no existe en el catálogo.");

            requisition.IdRequisitionStatus = targetStatus.IdRequisitionStatus;
            requisition.ReviewedDate = DateTime.Now;
            requisition.IdReviewedByUser = reviewedByUserId;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelRequisitionAsync(int id, int userId, string reason)
    {
        return await UpdateStatusAsync(id, RequisitionStatusCodes.Cancelado, userId, reason);
    }
}
