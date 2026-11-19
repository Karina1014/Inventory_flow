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

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly AplicationDbContext _db;
    private readonly IPurchaseOrderCatalogRepository _catalog;

    public PurchaseOrderRepository(AplicationDbContext db, IPurchaseOrderCatalogRepository catalog)
    {
        _db = db;
        _catalog = catalog;
    }

    private IQueryable<PurchaseOrder> QueryWithIncludes()
    {
        return _db.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.DestinationWarehouse)
            .Include(o => o.User)
            .Include(o => o.FinancialApprovalUser)
            .Include(o => o.PurchaseOrderType)
            .Include(o => o.PurchaseOrderStatus)
            .Include(o => o.Details)
                .ThenInclude(d => d.Product!)
                    .ThenInclude(p => p.Category)
            .Include(o => o.Details)
                .ThenInclude(d => d.InternalRequisition);
    }

    public async Task<PagedResultDto<PurchaseOrder>> GetOrdersPagedAsync(
        int pageNumber,
        int pageSize,
        int? supplierId = null,
        int? statusId = null,
        int? orderTypeId = null,
        string? search = null,
        bool pendingFinancialOnly = false)
    {
        var query = QueryWithIncludes().AsNoTracking().AsSplitQuery();

        if (pendingFinancialOnly)
        {
            query = query.Where(o => o.IdPurchaseOrderStatus == 2);
        }
        else if (statusId.HasValue && statusId.Value > 0)
        {
            query = query.Where(o => o.IdPurchaseOrderStatus == statusId.Value);
        }

        if (supplierId.HasValue && supplierId.Value > 0)
        {
            query = query.Where(o => o.IdSupplier == supplierId.Value);
        }

        if (orderTypeId.HasValue && orderTypeId.Value > 0)
        {
            query = query.Where(o => o.IdPurchaseOrderType == orderTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(term) ||
                (o.Supplier != null && o.Supplier.CompanyName.ToLower().Contains(term)) ||
                (o.Supplier != null && o.Supplier.Ruc.Contains(term)) ||
                o.Observation.ToLower().Contains(term));
        }

        IOrderedQueryable<PurchaseOrder> ordered = pendingFinancialOnly
            // En bandeja financiera: compras críticas primero, luego por monto descendente
            ? query.OrderBy(o => o.PurchaseOrderType!.PriorityLevel).ThenByDescending(o => o.TotalAmount)
            // En listado general: fecha más reciente primero
            : query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.IdPurchaseOrder);

        return await ordered.ToPagedResultAsync(pageNumber, pageSize);
    }

    public async Task<PurchaseOrder?> GetOrderByIdAsync(int id)
    {
        return await QueryWithIncludes()
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.IdPurchaseOrder == id);
    }

    public async Task<string> GenerateNextOrderNumberAsync()
    {
        var prefix = $"OC-{DateTime.Now.Year}-";

        var lastOrder = await _db.PurchaseOrders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.IdPurchaseOrder)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        var nextSeq = 1;
        if (!string.IsNullOrEmpty(lastOrder))
        {
            var parts = lastOrder.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var currentSeq))
            {
                nextSeq = currentSeq + 1;
            }
        }

        return $"{prefix}{nextSeq:D4}";
    }

    public async Task<PurchaseOrder> CreateOrderAsync(CreatePurchaseOrderDto dto, int userId)
    {
        var supplier = await _db.Suppliers.FindAsync(dto.IdSupplier);
        if (supplier == null || !supplier.IsActive)
        {
            throw new InvalidOperationException("El proveedor seleccionado no existe o está inactivo.");
        }

        var type = await _catalog.GetTypeByIdAsync(dto.IdPurchaseOrderType);
        if (type == null || !type.IsActive)
        {
            throw new InvalidOperationException("El tipo de orden de compra no existe o está inactivo.");
        }

        // Id 2 = Pendiente Aprobación Financiera
        var initialStatus = await _catalog.GetStatusByIdAsync(2)
            ?? await _db.PurchaseOrderStatuses.OrderBy(s => s.SortOrder).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No se encontró el estado inicial en el catálogo de órdenes de compra.");

        // Destino: si no viene especificado, se destina a la bodega matriz (Carapungo)
        var destinationWarehouseId = dto.IdDestinationWarehouse;
        if (!destinationWarehouseId.HasValue || destinationWarehouseId.Value <= 0)
        {
            var matriz = await _db.Warehouses
                .Where(w => w.IsActive)
                .FirstOrDefaultAsync(w => w.Name.Contains("Carapungo")
                                       || w.Name.Contains("Matriz")
                                       || w.Name.Contains("Principal")
                                       || w.Name.Contains("Central")
                                       || w.Address.Contains("Carapungo"))
                ?? await _db.Warehouses.Where(w => w.IsActive).OrderBy(w => w.IdWarehouse).FirstOrDefaultAsync();

            destinationWarehouseId = matriz?.IdWarehouse
                ?? throw new InvalidOperationException("No se encontró la bodega matriz Carapungo para recepcionar la orden.");
        }

        // Calcular fecha estimada de entrega según tiempo de entrega del proveedor si no se especificó
        var expectedDate = dto.ExpectedDeliveryDate ?? DateTime.Now.AddDays(Math.Max(1, supplier.DeliveryLeadTimeDays));

        var order = new PurchaseOrder
        {
            OrderNumber = await GenerateNextOrderNumberAsync(),
            OrderDate = DateTime.Now,
            IdSupplier = supplier.IdSupplier,
            IdDestinationWarehouse = destinationWarehouseId.Value,
            IdUser = userId,
            IdPurchaseOrderType = type.IdPurchaseOrderType,
            IdPurchaseOrderStatus = initialStatus.IdPurchaseOrderStatus,
            Observation = dto.Observation?.Trim() ?? string.Empty,
            ExpectedDeliveryDate = expectedDate
        };

        decimal runningSubtotal = 0;
        decimal runningTax = 0;

        foreach (var detailDto in dto.Details.Where(d => d.Quantity > 0))
        {
            var lineSubtotal = Math.Round(detailDto.Quantity * detailDto.UnitPrice, 2);
            var lineTax = Math.Round(lineSubtotal * (detailDto.TaxPercentage / 100m), 2);
            var lineTotal = lineSubtotal + lineTax;

            runningSubtotal += lineSubtotal;
            runningTax += lineTax;

            order.Details.Add(new PurchaseOrderDetail
            {
                IdProduct = detailDto.IdProduct,
                Quantity = detailDto.Quantity,
                ReceivedQuantity = 0,
                UnitPrice = detailDto.UnitPrice,
                Subtotal = lineSubtotal,
                TaxPercentage = detailDto.TaxPercentage,
                TaxAmount = lineTax,
                Total = lineTotal,
                Notes = detailDto.Notes?.Trim() ?? string.Empty,
                IdInternalRequisition = detailDto.IdInternalRequisition
            });
        }

        if (order.Details.Count == 0)
        {
            throw new InvalidOperationException("La orden de compra debe tener al menos un ítem con cantidad válida.");
        }

        order.Subtotal = runningSubtotal;
        order.TaxAmount = runningTax;
        order.TotalAmount = runningSubtotal + runningTax;

        _db.PurchaseOrders.Add(order);
        await _db.SaveChangesAsync();

        return order;
    }

    public async Task<bool> ProcessFinancialApprovalAsync(int id, bool approved, int financialUserId, string? note = null)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.PurchaseOrderStatus)
            .FirstOrDefaultAsync(o => o.IdPurchaseOrder == id);

        if (order == null) return false;

        if (order.PurchaseOrderStatus != null && order.PurchaseOrderStatus.IsFinal)
        {
            throw new InvalidOperationException(
                $"No se puede procesar la resolución financiera porque la orden ya está en estado final '{order.PurchaseOrderStatus.Name}'.");
        }

        // Id 3 = Aprobado por Finanzas, Id 4 = Rechazado por Finanzas
        var targetStatusId = approved ? 3 : 4;
        var targetStatus = await _catalog.GetStatusByIdAsync(targetStatusId)
            ?? await _db.PurchaseOrderStatuses.FirstOrDefaultAsync(s => s.IdPurchaseOrderStatus == targetStatusId)
            ?? throw new InvalidOperationException($"El estado con ID {targetStatusId} no está configurado en el catálogo.");

        order.IdPurchaseOrderStatus = targetStatus.IdPurchaseOrderStatus;
        order.FinancialApprovalDate = DateTime.Now;
        order.IdFinancialApprovalUser = financialUserId;
        order.FinancialApprovalNote = note?.Trim() ?? (approved ? "Aprobado por Dirección Financiera" : "Rechazado por Dirección Financiera");

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int id, string newStatusCode, int userId, string? note = null)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.PurchaseOrderStatus)
            .FirstOrDefaultAsync(o => o.IdPurchaseOrder == id);

        if (order == null) return false;

        if (order.PurchaseOrderStatus != null && order.PurchaseOrderStatus.IsFinal)
        {
            throw new InvalidOperationException(
                $"No se puede cambiar el estado porque la orden ya está en estado final '{order.PurchaseOrderStatus.Name}'.");
        }

        var targetStatus = await _catalog.GetStatusByCodeAsync(newStatusCode)
            ?? throw new InvalidOperationException($"El estado '{newStatusCode}' no existe en el catálogo.");

        order.IdPurchaseOrderStatus = targetStatus.IdPurchaseOrderStatus;
        if (!string.IsNullOrWhiteSpace(note))
        {
            order.Observation = $"{order.Observation} | {note.Trim()}".Trim(' ', '|');
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelOrderAsync(int id, int userId, string reason)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.PurchaseOrderStatus)
            .FirstOrDefaultAsync(o => o.IdPurchaseOrder == id);

        if (order == null) return false;

        if (order.PurchaseOrderStatus != null && order.PurchaseOrderStatus.IsFinal)
        {
            throw new InvalidOperationException(
                $"No se puede anular la orden porque ya está en estado final '{order.PurchaseOrderStatus.Name}'.");
        }

        // Id 8 = Cancelado
        var cancelStatus = await _catalog.GetStatusByIdAsync(8)
            ?? await _db.PurchaseOrderStatuses.FirstOrDefaultAsync(s => s.Code == "CANCELADO")
            ?? throw new InvalidOperationException("No se encontró el estado de cancelación en el catálogo.");

        order.IdPurchaseOrderStatus = cancelStatus.IdPurchaseOrderStatus;
        order.Observation = $"{order.Observation} | Anulada: {reason.Trim()}".Trim(' ', '|');

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReceiveOrderShipmentAsync(ReceivePurchaseOrderDto dto, int userId)
    {
        if (dto == null || dto.Items == null || !dto.Items.Any(i => i.QuantityToReceive > 0))
        {
            throw new InvalidOperationException("Debe ingresar al menos un ítem con cantidad mayor a cero para recepcionar.");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = await _db.PurchaseOrders
                .Include(o => o.PurchaseOrderStatus)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.IdPurchaseOrder == dto.IdPurchaseOrder);

            if (order == null)
            {
                throw new InvalidOperationException($"La orden de compra #{dto.IdPurchaseOrder} no existe.");
            }

            // Validar que la orden esté en un estado apto para recepcionar mercadería
            var validCodes = new[] { "APROBADO_FINANCIERO", "EMITIDO_PROVEEDOR", "RECIBIDO_PARCIAL" };
            var currentCode = order.PurchaseOrderStatus?.Code ?? string.Empty;
            if (!validCodes.Contains(currentCode))
            {
                throw new InvalidOperationException(
                    $"No se puede registrar recepción física para una orden en estado '{order.PurchaseOrderStatus?.Name ?? currentCode}'. Debe estar aprobada o emitida al proveedor.");
            }

            // Buscar o crear tipo de movimiento de entrada
            var movementType = await _db.MovementTypes
                .FirstOrDefaultAsync(m => m.Code == "ENT_COMPRA" || m.Mode.ToLower() == "entrada" || m.Code == "ENT-01");

            if (movementType == null)
            {
                movementType = new MovementType
                {
                    Code = "ENT_COMPRA",
                    Mode = "Entrada",
                    Description = "Ingreso al inventario por compra a proveedor",
                    CreationDate = DateTime.Now
                };
                _db.MovementTypes.Add(movementType);
                await _db.SaveChangesAsync();
            }

            // Crear cabecera del movimiento de inventario
            var guideInfo = !string.IsNullOrWhiteSpace(dto.DeliveryNoteNumber) ? $" | Guía/Factura: {dto.DeliveryNoteNumber.Trim()}" : "";
            var notesInfo = !string.IsNullOrWhiteSpace(dto.ReceptionNotes) ? $" | Obs: {dto.ReceptionNotes.Trim()}" : "";

            var movement = new InventoryMovement
            {
                MovementDate = DateTime.Now,
                IdMovementType = movementType.IdMovementType,
                IdOriginWarehouse = null, // Entrada pura
                IdDestinationWarehouse = order.IdDestinationWarehouse,
                IdUser = userId,
                Observation = $"Recepción de Orden de Compra {order.OrderNumber}{guideInfo}{notesInfo}",
                ReferenceId = order.IdPurchaseOrder,
                ReferenceSource = "ORDEN_COMPRA"
            };

            _db.InventoryMovements.Add(movement);
            await _db.SaveChangesAsync();

            // Procesar cada ítem recibido
            foreach (var item in dto.Items.Where(i => i.QuantityToReceive > 0))
            {
                var detail = order.Details.FirstOrDefault(d => d.IdPurchaseOrderDetail == item.IdPurchaseOrderDetail);
                if (detail == null)
                {
                    throw new InvalidOperationException($"El detalle con ID {item.IdPurchaseOrderDetail} no pertenece a esta orden de compra.");
                }

                int remaining = Math.Max(0, detail.Quantity - detail.ReceivedQuantity);
                if (item.QuantityToReceive > remaining)
                {
                    throw new InvalidOperationException(
                        $"La cantidad a recibir ({item.QuantityToReceive}) para '{detail.Product?.Name}' excede la cantidad pendiente ({remaining}).");
                }

                if (string.IsNullOrWhiteSpace(item.LotNumber))
                {
                    throw new InvalidOperationException($"Debe ingresar el número de lote para el producto '{detail.Product?.Name}'.");
                }

                if (item.ExpirationDate <= DateTime.Today)
                {
                    throw new InvalidOperationException(
                        $"La fecha de caducidad ({item.ExpirationDate:dd/MM/yyyy}) para el lote '{item.LotNumber}' debe ser posterior a la fecha de hoy.");
                }

                var cleanLotNumber = item.LotNumber.Trim().ToUpperInvariant();

                // Buscar si ya existe el lote en ese almacén con esa caducidad
                var lot = await _db.Lots.FirstOrDefaultAsync(l =>
                    l.IdProduct == detail.IdProduct &&
                    l.IdWarehouse == order.IdDestinationWarehouse &&
                    l.LotNumber == cleanLotNumber &&
                    l.ExpirationDate.Date == item.ExpirationDate.Date);

                if (lot != null)
                {
                    lot.Quantity += item.QuantityToReceive;
                    lot.IsActive = true;
                }
                else
                {
                    lot = new Lot
                    {
                        IdProduct = detail.IdProduct,
                        IdWarehouse = order.IdDestinationWarehouse,
                        LotNumber = cleanLotNumber,
                        Quantity = item.QuantityToReceive,
                        ExpirationDate = item.ExpirationDate.Date,
                        IsActive = true,
                        CreationDate = DateTime.Now
                    };
                    _db.Lots.Add(lot);
                    await _db.SaveChangesAsync();
                }

                // Actualizar detalle de la orden
                detail.ReceivedQuantity += item.QuantityToReceive;

                // Crear detalle del movimiento de inventario
                var movementDetail = new InventoryMovementDetail
                {
                    IdInventoryMovement = movement.IdInventoryMovement,
                    IdProduct = detail.IdProduct,
                    IdLot = lot.IdLot,
                    Quantity = item.QuantityToReceive,
                    UnitCost = detail.UnitPrice
                };
                _db.InventoryMovementDetails.Add(movementDetail);
            }

            // Determinar si la orden se completó en su totalidad o parcialmente
            bool allReceived = order.Details.All(d => d.ReceivedQuantity >= d.Quantity);
            var targetStatusCode = allReceived ? "RECIBIDO_TOTAL" : "RECIBIDO_PARCIAL";

            var targetStatus = await _catalog.GetStatusByCodeAsync(targetStatusCode)
                ?? await _db.PurchaseOrderStatuses.FirstOrDefaultAsync(s => s.Code == targetStatusCode)
                ?? throw new InvalidOperationException($"No se encontró el estado '{targetStatusCode}' en el catálogo.");

            order.IdPurchaseOrderStatus = targetStatus.IdPurchaseOrderStatus;

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
}
