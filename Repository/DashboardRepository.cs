using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly AplicationDbContext _db;

    public DashboardRepository(AplicationDbContext db)
    {
        _db = db;
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

        // 5. Productos con bajo stock (suma de stock en todos los lotes <= 5 unidades o sin stock)
        var productStocks = activeLots
            .GroupBy(l => l.IdProduct)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

        var activeProducts = await _db.Products.Where(p => p.IsActive).ToListAsync();
        int lowStockCount = 0;
        foreach (var prod in activeProducts)
        {
            int currentStock = productStocks.TryGetValue(prod.IdProduct, out int qty) ? qty : 0;
            if (currentStock <= 5)
            {
                lowStockCount++;
            }
        }
        metrics.LowStockProductsCount = lowStockCount;

        // 6. Stock por Categoría (con porcentaje respecto al total)
        var categories = await _db.Categories.ToListAsync();
        var categoryStockList = new List<CategoryStockDto>();
        int grandTotalUnits = activeLots.Sum(l => l.Quantity);

        foreach (var cat in categories)
        {
            var unitsInCat = activeLots
                .Where(l => l.Product != null && l.Product.IdCategory == cat.IdCategory)
                .Sum(l => l.Quantity);

            if (unitsInCat > 0 || grandTotalUnits == 0)
            {
                categoryStockList.Add(new CategoryStockDto
                {
                    IdCategory = cat.IdCategory,
                    CategoryName = cat.Name,
                    TotalUnits = unitsInCat,
                    Percentage = grandTotalUnits > 0 ? Math.Round((double)unitsInCat / grandTotalUnits * 100, 1) : 0
                });
            }
        }
        metrics.StockByCategory = categoryStockList.OrderByDescending(c => c.TotalUnits).ToList();

        // 7. Actividad Reciente (Últimos 6 movimientos)
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

