using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Inventory_Flow.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventory_Flow.Repository;

public class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly AplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IN8nNotificationService _n8nService;
    private readonly INotificationRecipientService _recipientService;
    private readonly IConfiguration _config;
    private readonly ILogger<InventoryMovementRepository> _logger;

    public InventoryMovementRepository(
        AplicationDbContext db,
        IMapper mapper,
        IN8nNotificationService n8nService,
        INotificationRecipientService recipientService,
        IConfiguration config,
        ILogger<InventoryMovementRepository> logger)
    {
        _db = db;
        _mapper = mapper;
        _n8nService = n8nService;
        _recipientService = recipientService;
        _config = config;
        _logger = logger;
    }

    public async Task<ICollection<InventoryMovementDto>> GetMovementsAsync()
    {
        var movements = await _db.InventoryMovements
            .Include(m => m.MovementType)
            .Include(m => m.OriginWarehouse)
            .Include(m => m.DestinationWarehouse)
            .Include(m => m.User)
            .Include(m => m.Details)
                .ThenInclude(d => d.Product)
            .Include(m => m.Details)
                .ThenInclude(d => d.Lot)
            .OrderByDescending(m => m.MovementDate)
            .ThenByDescending(m => m.IdInventoryMovement)
            .ToListAsync();

        return _mapper.Map<List<InventoryMovementDto>>(movements);
    }

    public async Task<InventoryMovementDto?> GetMovementByIdAsync(int id)
    {
        var movement = await _db.InventoryMovements
            .Include(m => m.MovementType)
            .Include(m => m.OriginWarehouse)
            .Include(m => m.DestinationWarehouse)
            .Include(m => m.User)
            .Include(m => m.Details)
                .ThenInclude(d => d.Product)
            .Include(m => m.Details)
                .ThenInclude(d => d.Lot)
            .FirstOrDefaultAsync(m => m.IdInventoryMovement == id);

        return movement == null ? null : _mapper.Map<InventoryMovementDto>(movement);
    }

    public async Task<InventoryMovement> CreateMovementAsync(CreateInventoryMovementDto dto)
    {
        var movementType = await _db.MovementTypes.FindAsync(dto.IdMovementType)
            ?? throw new InvalidOperationException($"El tipo de movimiento {dto.IdMovementType} no existe.");

        var mode = movementType.Mode.ToUpperInvariant();

        if (mode == "E")
        {
            if (!dto.IdDestinationWarehouse.HasValue)
                throw new InvalidOperationException("Un movimiento de entrada debe tener un almacén de destino.");
            dto.IdOriginWarehouse = null;
        }
        else if (mode == "S")
        {
            if (!dto.IdOriginWarehouse.HasValue)
                throw new InvalidOperationException("Un movimiento de salida debe tener un almacén de origen.");
            dto.IdDestinationWarehouse = null;
        }
        else if (mode == "T")
        {
            if (!dto.IdOriginWarehouse.HasValue || !dto.IdDestinationWarehouse.HasValue)
                throw new InvalidOperationException("Una transferencia requiere almacén de origen y de destino.");
            if (dto.IdOriginWarehouse == dto.IdDestinationWarehouse)
                throw new InvalidOperationException("El almacén de origen y destino no pueden ser el mismo.");
        }

        if (dto.Details == null || dto.Details.Count == 0)
            throw new InvalidOperationException("El movimiento debe contener al menos un detalle de producto.");

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var movement = _mapper.Map<InventoryMovement>(dto);

            foreach (var detail in movement.Details)
            {
                if (detail.IdLot.HasValue)
                {
                    var sourceLot = await _db.Lots.FindAsync(detail.IdLot.Value);
                    if (sourceLot != null)
                    {
                        if (mode == "S")
                        {
                            if (sourceLot.Quantity < detail.Quantity)
                                throw new InvalidOperationException($"Stock insuficiente en el lote {sourceLot.LotNumber}. Disponible: {sourceLot.Quantity}, solicitado: {detail.Quantity}");
                            sourceLot.Quantity -= detail.Quantity;
                        }
                        else if (mode == "E")
                        {
                            sourceLot.Quantity += detail.Quantity;
                        }
                        else if (mode == "T")
                        {
                            if (sourceLot.Quantity < detail.Quantity)
                                throw new InvalidOperationException($"Stock insuficiente en el lote {sourceLot.LotNumber} para transferir. Disponible: {sourceLot.Quantity}, solicitado: {detail.Quantity}");
                            
                            // Restar del lote en almacén origen
                            sourceLot.Quantity -= detail.Quantity;

                            // Buscar o crear el lote correspondiente en el almacén destino
                            var destLot = await _db.Lots.FirstOrDefaultAsync(l => 
                                l.IdProduct == detail.IdProduct && 
                                l.IdWarehouse == dto.IdDestinationWarehouse!.Value && 
                                l.LotNumber.ToLower() == sourceLot.LotNumber.ToLower());

                            if (destLot != null)
                            {
                                destLot.Quantity += detail.Quantity;
                            }
                            else
                            {
                                destLot = new Lot
                                {
                                    IdProduct = detail.IdProduct,
                                    IdWarehouse = dto.IdDestinationWarehouse!.Value,
                                    LotNumber = sourceLot.LotNumber,
                                    Quantity = detail.Quantity,
                                    ExpirationDate = sourceLot.ExpirationDate,
                                    CreationDate = DateTime.Now
                                };
                                _db.Lots.Add(destLot);
                            }
                        }
                    }
                }
            }

            _db.InventoryMovements.Add(movement);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            // Verificación y despacho de alerta en tiempo real tras egreso/venta
            if (mode == "S" && dto.IdOriginWarehouse.HasValue)
            {
                await CheckAndNotifyLowStockAsync(dto.IdOriginWarehouse.Value, movement.Details, movement.IdUser ?? 1);
            }

            return movement;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task CheckAndNotifyLowStockAsync(int idWarehouse, ICollection<InventoryMovementDetail> details, int idUser)
    {
        try
        {
            var thresholdEntity = await _db.ThresholdInvetories.AsNoTracking().FirstOrDefaultAsync(t => t.IsActive);
            int threshold = thresholdEntity?.Threshold ?? 5;

            var productIds = details.Select(d => d.IdProduct).Distinct().ToList();

            var warehouse = await _db.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.IdWarehouse == idWarehouse);
            var warehouseName = warehouse?.Name ?? $"Sucursal #{idWarehouse}";

            // Resolver dinámicamente el teléfono del Bodeguero Principal / Admin según rol en la base de datos
            var recipient = await _recipientService.GetBodegueroPrincipalPhoneAsync();

            foreach (var prodId in productIds)
            {
                var remainingStock = await _db.Lots
                    .AsNoTracking()
                    .Where(l => l.IdWarehouse == idWarehouse && l.IdProduct == prodId && l.IsActive && l.Quantity > 0)
                    .SumAsync(l => (int?)l.Quantity) ?? 0;

                if (remainingStock <= threshold)
                {
                    var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.IdProduct == prodId);
                    var prodName = product?.Name ?? "Producto";
                    var prodCode = product?.Code ?? "-";

                    await _n8nService.SendLowStockSaleAlertAsync(
                        idWarehouse,
                        warehouseName,
                        prodId,
                        prodCode,
                        prodName,
                        remainingStock,
                        threshold,
                        recipient,
                        idUser
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar y despachar alerta de stock crítico tras venta en almacén {WarehouseId}", idWarehouse);
        }
    }

    public async Task<InventoryMovement> UpdateMovementAsync(int id, UpdateInventoryMovementDto dto)
    {
        var movement = await _db.InventoryMovements.FindAsync(id)
            ?? throw new KeyNotFoundException($"Movimiento con Id {id} no encontrado.");

        movement.Observation = dto.Observation ?? string.Empty;
        movement.ReferenceId = dto.ReferenceId;
        movement.ReferenceSource = dto.ReferenceSource;

        _db.InventoryMovements.Update(movement);
        await _db.SaveChangesAsync();
        return movement;
    }

    public async Task<bool> DeleteMovementAsync(int id)
    {
        var movement = await _db.InventoryMovements
            .Include(m => m.Details)
            .FirstOrDefaultAsync(m => m.IdInventoryMovement == id);

        if (movement == null) return false;

        _db.InventoryMovements.Remove(movement);
        await _db.SaveChangesAsync();
        return true;
    }
}
