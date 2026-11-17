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
                .FirstOrDefaultAsync(w => w.Name.Contains("Carapungo") || w.Name.Contains("Matriz") || w.Name.Contains("Central"));

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
}
