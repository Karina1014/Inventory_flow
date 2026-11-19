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

        var initialStatus = await _catalog.GetStatusByIdAsync(1)
            ?? await _db.RequisitionStatuses.OrderBy(s => s.SortOrder).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No está configurado el estado inicial en el catálogo de requisiciones.");

        // Si no se especifica la bodega proveedora, se usa la matriz (Carapungo)
        var fulfillingWarehouseId = dto.IdFulfillingWarehouse;
        if (!fulfillingWarehouseId.HasValue || fulfillingWarehouseId.Value <= 0)
        {
            var matriz = await _db.Warehouses
                .Where(w => w.IsActive)
                .FirstOrDefaultAsync(w => w.Name.Contains("Carapungo")
                                       || w.Name.Contains("Matriz")
                                       || w.Name.Contains("Principal")
                                       || w.Name.Contains("Central")
                                       || w.Address.Contains("Carapungo"))
                ?? await _db.Warehouses.Where(w => w.IsActive).OrderBy(w => w.IdWarehouse).FirstOrDefaultAsync();

            fulfillingWarehouseId = matriz?.IdWarehouse
                ?? throw new InvalidOperationException("No se encontró la bodega matriz (Carapungo) para atender la requisición.");
        }

        if (fulfillingWarehouseId.Value == dto.IdRequestingWarehouse)
        {
            throw new InvalidOperationException("La sucursal solicitante no puede ser la misma Bodega Matriz Principal (Carapungo). Por favor selecciona una sucursal (ej. Llano Grande o Pusuquí).");
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

        if (requisition.RequisitionStatus != null && requisition.RequisitionStatus.IsFinal)
        {
            throw new InvalidOperationException(
                $"No se puede cambiar el estado porque la requisición ya está en estado final '{requisition.RequisitionStatus.Name}'.");
        }

        var newStatus = await _catalog.GetStatusByCodeAsync(newStatusCode)
            ?? throw new InvalidOperationException($"El estado '{newStatusCode}' no existe en el catálogo.");

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
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var requisition = await _db.InternalRequisitions
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.RequisitionStatus)
                .Include(r => r.RequestingWarehouse)
                .Include(r => r.FulfillingWarehouse)
                .FirstOrDefaultAsync(r => r.IdInternalRequisition == id);

            if (requisition == null) return false;

            if (requisition.RequisitionStatus != null && requisition.RequisitionStatus.IsFinal)
            {
                throw new InvalidOperationException(
                    $"No se pueden actualizar despachos porque la requisición ya está en estado final '{requisition.RequisitionStatus.Name}'.");
            }

            var movementType = await _db.MovementTypes
                .FirstOrDefaultAsync(m => m.Code == "20" || m.Code == "05" || m.Mode == "T")
                ?? await _db.MovementTypes.FirstOrDefaultAsync();

            if (movementType == null)
            {
                throw new InvalidOperationException("No se encontró un tipo de movimiento de transferencia/traslado configurado.");
            }

            var outgoingMovement = new InventoryMovement
            {
                MovementDate = DateTime.Now,
                IdMovementType = movementType.IdMovementType,
                IdOriginWarehouse = requisition.IdFulfillingWarehouse,
                IdDestinationWarehouse = requisition.IdRequestingWarehouse,
                Observation = $"Despacho de Requisición #{requisition.RequisitionCode} hacia {requisition.RequestingWarehouse?.Name} (En Tránsito)",
                IdUser = reviewedByUserId,
                ReferenceSource = "REQUISICION_INTERNA_DESPACHO",
                ReferenceId = id
            };

            _db.InventoryMovements.Add(outgoingMovement);
            await _db.SaveChangesAsync();

            bool hasAnyDispatch = false;

            foreach (var detail in requisition.Details)
            {
                if (productDispatchedQuantities.TryGetValue(detail.IdProduct, out var qty) && qty > 0)
                {
                    int requestedToDispatch = Math.Min(qty, detail.RequestedQuantity);

                    // Consultar lotes con existencia en Carapungo ordenados por FEFO
                    var availableLots = await _db.Lots
                        .Where(l => l.IdWarehouse == requisition.IdFulfillingWarehouse
                                 && l.IdProduct == detail.IdProduct
                                 && l.IsActive
                                 && l.Quantity > 0)
                        .OrderBy(l => l.ExpirationDate)
                        .ToListAsync();

                    int totalAvailableStock = availableLots.Sum(l => l.Quantity);
                    if (totalAvailableStock < requestedToDispatch)
                    {
                        var prodName = detail.Product?.Name ?? $"ID {detail.IdProduct}";
                        throw new InvalidOperationException(
                            $"Stock insuficiente en Carapungo para '{prodName}'. Solicitado despachar: {requestedToDispatch}, Disponible en lotes: {totalAvailableStock}.");
                    }

                    int remainingToDeduct = requestedToDispatch;
                    foreach (var lot in availableLots)
                    {
                        if (remainingToDeduct <= 0) break;

                        int take = Math.Min(lot.Quantity, remainingToDeduct);
                        lot.Quantity -= take;
                        remainingToDeduct -= take;

                        outgoingMovement.Details.Add(new InventoryMovementDetail
                        {
                            IdInventoryMovement = outgoingMovement.IdInventoryMovement,
                            IdProduct = detail.IdProduct,
                            IdLot = lot.IdLot,
                            Quantity = take,
                            UnitCost = detail.Product?.SuggestedPrice ?? 0
                        });
                    }

                    detail.DispatchedQuantity = requestedToDispatch;
                    hasAnyDispatch = true;
                }
                else
                {
                    detail.DispatchedQuantity = 0;
                }
            }

            if (!hasAnyDispatch)
            {
                throw new InvalidOperationException("Debes ingresar al menos una cantidad mayor a 0 para realizar el despacho.");
            }

            var totalReq = requisition.Details.Sum(d => d.RequestedQuantity);
            var totalDisp = requisition.Details.Sum(d => d.DispatchedQuantity);

            // Id 4 = Despachado (total), Id 3 = Aprobado Parcial
            var targetStatusId = totalDisp >= totalReq ? 4 : 3;
            var targetStatus = await _catalog.GetStatusByIdAsync(targetStatusId)
                ?? await _db.RequisitionStatuses.FirstOrDefaultAsync(s => s.IdRequisitionStatus == targetStatusId);

            if (targetStatus != null)
            {
                requisition.IdRequisitionStatus = targetStatus.IdRequisitionStatus;
                requisition.ReviewedDate = DateTime.Now;
                requisition.IdReviewedByUser = reviewedByUserId;
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ConfirmBranchReceiptAsync(int id, int userId, string? confirmationNote = null)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var requisition = await _db.InternalRequisitions
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.RequisitionStatus)
                .Include(r => r.RequestingWarehouse)
                .Include(r => r.FulfillingWarehouse)
                .FirstOrDefaultAsync(r => r.IdInternalRequisition == id);

            if (requisition == null) return false;

            // Validar que se encuentre en estado DESPACHADO (Id 4)
            if (requisition.RequisitionStatus?.Code != "DESPACHADO" && requisition.IdRequisitionStatus != 4)
            {
                throw new InvalidOperationException($"La requisición no está en tránsito/despachada. Su estado actual es '{requisition.RequisitionStatus?.Name}'.");
            }

            // Buscar el movimiento de salida generado en el despacho para obtener los lotes exactos
            var outgoingMovement = await _db.InventoryMovements
                .Include(m => m.Details)
                    .ThenInclude(d => d.Lot)
                .FirstOrDefaultAsync(m => m.ReferenceSource == "REQUISICION_INTERNA_DESPACHO" && m.ReferenceId == id);

            var movementType = await _db.MovementTypes
                .FirstOrDefaultAsync(m => m.Code == "05" || m.Code == "20" || m.Mode == "T")
                ?? await _db.MovementTypes.FirstOrDefaultAsync();

            if (movementType == null)
            {
                throw new InvalidOperationException("No se encontró un tipo de movimiento de transferencia configurado.");
            }

            var incomingMovement = new InventoryMovement
            {
                MovementDate = DateTime.Now,
                IdMovementType = movementType.IdMovementType,
                IdOriginWarehouse = requisition.IdFulfillingWarehouse,
                IdDestinationWarehouse = requisition.IdRequestingWarehouse,
                Observation = $"Recepción de Transferencia Requisición #{requisition.RequisitionCode} confirmada en {requisition.RequestingWarehouse?.Name}. {confirmationNote}".Trim(),
                IdUser = userId,
                ReferenceSource = "REQUISICION_INTERNA_RECEPCION",
                ReferenceId = id
            };

            _db.InventoryMovements.Add(incomingMovement);
            await _db.SaveChangesAsync();

            if (outgoingMovement != null && outgoingMovement.Details.Any())
            {
                foreach (var outDetail in outgoingMovement.Details)
                {
                    var lotNumber = outDetail.Lot?.LotNumber ?? "S/N";
                    var expDate = outDetail.Lot?.ExpirationDate ?? DateTime.Today.AddYears(2);

                    // Buscar si existe el mismo lote en la sucursal de destino
                    var branchLot = await _db.Lots
                        .FirstOrDefaultAsync(l => l.IdWarehouse == requisition.IdRequestingWarehouse
                                               && l.IdProduct == outDetail.IdProduct
                                               && l.LotNumber == lotNumber);

                    if (branchLot != null)
                    {
                        branchLot.Quantity += outDetail.Quantity;
                        branchLot.IsActive = true;
                    }
                    else
                    {
                        branchLot = new Lot
                        {
                            IdProduct = outDetail.IdProduct,
                            IdWarehouse = requisition.IdRequestingWarehouse,
                            LotNumber = lotNumber,
                            Quantity = outDetail.Quantity,
                            ExpirationDate = expDate,
                            CreationDate = DateTime.Now,
                            IsActive = true
                        };
                        _db.Lots.Add(branchLot);
                        await _db.SaveChangesAsync();
                    }

                    incomingMovement.Details.Add(new InventoryMovementDetail
                    {
                        IdInventoryMovement = incomingMovement.IdInventoryMovement,
                        IdProduct = outDetail.IdProduct,
                        IdLot = branchLot.IdLot,
                        Quantity = outDetail.Quantity,
                        UnitCost = outDetail.UnitCost
                    });
                }
            }
            else
            {
                // Respaldo en caso de que no hubiese detalle previo
                foreach (var detail in requisition.Details.Where(d => d.DispatchedQuantity > 0))
                {
                    var branchLot = await _db.Lots
                        .FirstOrDefaultAsync(l => l.IdWarehouse == requisition.IdRequestingWarehouse
                                               && l.IdProduct == detail.IdProduct
                                               && l.LotNumber == $"TRANSF-{requisition.RequisitionCode}");

                    if (branchLot != null)
                    {
                        branchLot.Quantity += detail.DispatchedQuantity;
                        branchLot.IsActive = true;
                    }
                    else
                    {
                        branchLot = new Lot
                        {
                            IdProduct = detail.IdProduct,
                            IdWarehouse = requisition.IdRequestingWarehouse,
                            LotNumber = $"TRANSF-{requisition.RequisitionCode}",
                            Quantity = detail.DispatchedQuantity,
                            ExpirationDate = DateTime.Today.AddYears(2),
                            CreationDate = DateTime.Now,
                            IsActive = true
                        };
                        _db.Lots.Add(branchLot);
                        await _db.SaveChangesAsync();
                    }

                    incomingMovement.Details.Add(new InventoryMovementDetail
                    {
                        IdInventoryMovement = incomingMovement.IdInventoryMovement,
                        IdProduct = detail.IdProduct,
                        IdLot = branchLot.IdLot,
                        Quantity = detail.DispatchedQuantity,
                        UnitCost = detail.Product?.SuggestedPrice ?? 0
                    });
                }
            }

            // Estado 5 = COMPLETADO
            var completedStatus = await _catalog.GetStatusByIdAsync(5)
                ?? await _db.RequisitionStatuses.FirstOrDefaultAsync(s => s.Code == "COMPLETADO");

            if (completedStatus != null)
            {
                requisition.IdRequisitionStatus = completedStatus.IdRequisitionStatus;
            }

            requisition.ReviewedDate = DateTime.Now;
            requisition.IdReviewedByUser = userId;
            if (!string.IsNullOrWhiteSpace(confirmationNote))
            {
                requisition.ReviewNote = string.IsNullOrWhiteSpace(requisition.ReviewNote)
                    ? confirmationNote.Trim()
                    : $"{requisition.ReviewNote} | Recepción: {confirmationNote.Trim()}";
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> CancelRequisitionAsync(int id, int userId, string reason)
    {
        var requisition = await _db.InternalRequisitions
            .Include(r => r.RequisitionStatus)
            .FirstOrDefaultAsync(r => r.IdInternalRequisition == id);

        if (requisition == null) return false;

        if (requisition.RequisitionStatus != null && requisition.RequisitionStatus.IsFinal)
        {
            throw new InvalidOperationException(
                $"No se puede anular la requisición porque ya se encuentra en estado final '{requisition.RequisitionStatus.Name}'.");
        }

        // Id 7 = Cancelado
        var cancelStatus = await _catalog.GetStatusByIdAsync(7)
            ?? await _db.RequisitionStatuses.FirstOrDefaultAsync(s => s.Code == "CANCELADO")
            ?? throw new InvalidOperationException("No se encontró el estado de cancelación en el catálogo.");

        requisition.IdRequisitionStatus = cancelStatus.IdRequisitionStatus;
        requisition.ReviewedDate = DateTime.Now;
        requisition.IdReviewedByUser = userId;
        requisition.ReviewNote = string.IsNullOrWhiteSpace(reason) ? "Requisición anulada" : reason.Trim();

        await _db.SaveChangesAsync();
        return true;
    }
}
