using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Inventory_Flow.Services;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly AplicationDbContext _db;
    private readonly IThresholdInvetoryRepository _thresholdRepository;
    private readonly IConfiguration _configuration;

    public DashboardRepository(AplicationDbContext db, IThresholdInvetoryRepository thresholdRepository, IConfiguration configuration)
    {
        _db = db;
        _thresholdRepository = thresholdRepository;
        _configuration = configuration;
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
    {
        var metrics = new DashboardMetricsDto();

        // 1. Conteo de almacenes activos
        metrics.ActiveWarehousesCount = await _db.Warehouses.CountAsync(w => w.IsActive);

        // 2. Conteo de productos totales
        metrics.TotalProductsCount = await _db.Products.CountAsync(p => p.IsActive);

        // 3. Conteo de lotes y cálculo del Valor Total de Inventario
        var activeLots = await _db.Lots
            .Include(l => l.Product)
            .Where(l => l.Quantity > 0)
            .ToListAsync();

        metrics.TotalLotsCount = activeLots.Count;
        metrics.TotalInventoryValue = activeLots.Sum(l => l.Quantity * (l.Product?.SuggestedPrice ?? 0));

        // 4. Conteo de movimientos registrados el día de hoy
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        metrics.TodayMovementsCount = await _db.InventoryMovements
            .CountAsync(m => m.MovementDate >= today && m.MovementDate < tomorrow);

        // 5. Umbral de bajo stock dinámico: se toma el umbral activo (vigente) del listado
        //    de ThresholdInvetory a través de su repositorio (por defecto 5 si no hay activo)
        var currentThresholdEntity = await _thresholdRepository.GetCurrentThresholdAsync();

        int thresholdValue = currentThresholdEntity?.Threshold ?? 5;
        metrics.ActiveThreshold = thresholdValue;

        // Productos con bajo stock (suma de stock en todos los lotes <= umbral dinámico o sin stock)
        var productStocks = activeLots
            .GroupBy(l => l.IdProduct)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

        var activeProducts = await _db.Products.Where(p => p.IsActive).ToListAsync();
        int lowStockCount = 0;
        foreach (var prod in activeProducts)
        {
            int currentStock = productStocks.TryGetValue(prod.IdProduct, out int qty) ? qty : 0;
            if (currentStock <= thresholdValue)
            {
                lowStockCount++;
            }
        }
        metrics.LowStockProductsCount = lowStockCount;

        // 6. Lotes próximos a caducar (ventana configurable, 30 días por defecto)
        var alertDays = LotExpiration.GetAlertDays(_configuration);
        metrics.ExpirationAlertDays = alertDays;

        var expiringLots = await _db.Lots
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .Where(l => l.Quantity > 0 && l.ExpirationDate < DateTime.Today.AddDays(alertDays + 1))
            .OrderBy(l => l.ExpirationDate)
            .ToListAsync();

        metrics.ExpiredLotsCount = expiringLots.Count(l => l.ExpirationDate.Date < DateTime.Today);
        metrics.ExpiringLotsCount = expiringLots.Count(l => l.ExpirationDate.Date >= DateTime.Today);
        metrics.ExpiringSoonLots = expiringLots.Take(5).Select(l => new ExpiringLotDto
        {
            IdLot = l.IdLot,
            ProductName = l.Product?.Name ?? "Producto",
            LotNumber = l.LotNumber,
            WarehouseName = l.Warehouse?.Name ?? "-",
            ExpirationDate = l.ExpirationDate,
            DaysToExpire = LotExpiration.DaysToExpire(l.ExpirationDate),
            Quantity = l.Quantity
        }).ToList();

        // 7. Stock por Categoría (con porcentaje respecto al total)
        var categories = await _db.Categories.ToListAsync();
        var categoryStockList = new List<CategoryStockDto>();
        int grandTotalUnits = activeLots.Sum(l => l.Quantity);

        foreach (var cat in categories)
        {
            var unitsInCat = activeLots
                .Where(l => l.Product != null && l.Product.IdCategory == cat.IdCategory)
                .Sum(l => l.Quantity);

            // Se listan las categorías con stock y también las que están en o por debajo del umbral activo
            if (unitsInCat > 0 || grandTotalUnits == 0 || unitsInCat <= thresholdValue)
            {
                categoryStockList.Add(new CategoryStockDto
                {
                    IdCategory = cat.IdCategory,
                    CategoryName = cat.Name,
                    TotalUnits = unitsInCat,
                    Percentage = grandTotalUnits > 0 ? Math.Round((double)unitsInCat / grandTotalUnits * 100, 1) : 0,
                    IsLowStock = unitsInCat <= thresholdValue
                });
            }
        }
        metrics.StockByCategory = categoryStockList.OrderByDescending(c => c.TotalUnits).ToList();

        // 8. Actividad Reciente (Últimos 6 movimientos)
        var recentMovements = await _db.InventoryMovements
            .Include(m => m.MovementType)
            .Include(m => m.User)
            .Include(m => m.Details)
            .OrderByDescending(m => m.MovementDate)
            .ThenByDescending(m => m.IdInventoryMovement)
            .Take(6)
            .ToListAsync();

        var now = DateTime.Now;
        metrics.RecentActivities = recentMovements.Select(m => new RecentActivityDto
        {
            IdMovement = m.IdInventoryMovement,
            Date = m.MovementDate,
            TimeAgo = FormatTimeAgo(m.MovementDate, now),
            MovementTypeCode = m.MovementType?.Code ?? "",
            MovementTypeMode = m.MovementType?.Mode ?? "",
            Description = !string.IsNullOrWhiteSpace(m.Observation) 
                ? m.Observation 
                : $"{m.MovementType?.Description ?? "Movimiento"} #{m.IdInventoryMovement}",
            TotalItems = m.Details?.Sum(d => d.Quantity) ?? 0,
            UserName = m.User?.Name ?? "Sistema",
            IsAlert = m.MovementType != null && m.MovementType.Mode == "S"
        }).ToList();

        return metrics;
    }

    private static string FormatTimeAgo(DateTime pastDate, DateTime now)
    {
        var diff = now - pastDate;
        if (diff.TotalMinutes < 1) return "hace unos segundos";
        if (diff.TotalMinutes < 60) return $"hace {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24) return $"hace {(int)diff.TotalHours} h";
        if (diff.TotalDays < 30) return $"hace {(int)diff.TotalDays} d";
        return pastDate.ToString("dd/MM/yyyy");
    }
}

