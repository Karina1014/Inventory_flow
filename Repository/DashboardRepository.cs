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

        // 3. Conteo de lotes activos y cálculo del Valor Total de Inventario
        var activeLots = await _db.Lots
            .Include(l => l.Product)
            .Where(l => l.IsActive && l.Quantity > 0)
            .ToListAsync();

        metrics.TotalLotsCount = activeLots.Count;
        metrics.TotalInventoryValue = activeLots.Sum(l => l.Quantity * (l.Product?.SuggestedPrice ?? 0));

        // 4. Conteo de movimientos registrados el día de hoy y pendientes operativos
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        metrics.TodayMovementsCount = await _db.InventoryMovements
            .CountAsync(m => m.MovementDate >= today && m.MovementDate < tomorrow);

        metrics.PendingRequisitionsCount = await _db.InternalRequisitions
            .CountAsync(r => r.IdRequisitionStatus == 1 || r.IdRequisitionStatus == 2 || r.IdRequisitionStatus == 3);

        metrics.PendingPurchaseOrdersCount = await _db.PurchaseOrders
            .CountAsync(p => p.IdPurchaseOrderStatus == 2);

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
            .Where(l => l.IsActive && l.Quantity > 0 && l.ExpirationDate < DateTime.Today.AddDays(alertDays + 1))
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

        // 7. Tendencia Mensual (Últimos 6 meses) y Top Productos Más Vendidos / Despachados
        var sixMonthsAgo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
        var periodMovements = await _db.InventoryMovements
            .Include(m => m.MovementType)
            .Include(m => m.Details)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p!.Category)
            .AsNoTracking()
            .Where(m => m.MovementDate >= sixMonthsAgo)
            .ToListAsync();

        string[] shortMonthNames = { "", "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
        string[] fullMonthNames = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

        var monthlyList = new List<MonthlyMovementTrendDto>();
        for (int i = 5; i >= 0; i--)
        {
            var targetMonthDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-i);
            int y = targetMonthDate.Year;
            int m = targetMonthDate.Month;

            var monthMovs = periodMovements
                .Where(mov => mov.MovementDate.Year == y && mov.MovementDate.Month == m)
                .ToList();

            int entriesUnits = 0;
            int exitsUnits = 0;
            decimal entriesVal = 0;
            decimal exitsVal = 0;

            foreach (var mov in monthMovs)
            {
                string mode = mov.MovementType?.Mode ?? "E";
                bool isExitOrDispatch = mode == "S" || (mode == "T" && mov.ReferenceSource != "REQUISICION_INTERNA_RECEPCION");
                bool isEntry = mode == "E" || (mode == "T" && mov.ReferenceSource == "REQUISICION_INTERNA_RECEPCION");

                foreach (var det in mov.Details)
                {
                    decimal unitVal = det.UnitCost > 0 ? det.UnitCost : (det.Product?.SuggestedPrice ?? 0);
                    if (isExitOrDispatch)
                    {
                        exitsUnits += det.Quantity;
                        exitsVal += det.Quantity * unitVal;
                    }
                    else if (isEntry)
                    {
                        entriesUnits += det.Quantity;
                        entriesVal += det.Quantity * unitVal;
                    }
                }
            }

            monthlyList.Add(new MonthlyMovementTrendDto
            {
                Year = y,
                Month = m,
                MonthLabel = shortMonthNames[m],
                FullMonthName = $"{fullMonthNames[m]} {y}",
                EntriesUnits = entriesUnits,
                ExitsUnits = exitsUnits,
                EntriesValue = Math.Round(entriesVal, 2),
                ExitsValue = Math.Round(exitsVal, 2)
            });
        }

        int maxMonthlyUnits = monthlyList.SelectMany(x => new[] { x.EntriesUnits, x.ExitsUnits }).DefaultIfEmpty(1).Max();
        if (maxMonthlyUnits <= 0) maxMonthlyUnits = 1;

        foreach (var item in monthlyList)
        {
            item.EntriesHeightPercent = item.EntriesUnits > 0
                ? Math.Max(10, Math.Round((double)item.EntriesUnits / maxMonthlyUnits * 100, 1))
                : 4;
            item.ExitsHeightPercent = item.ExitsUnits > 0
                ? Math.Max(10, Math.Round((double)item.ExitsUnits / maxMonthlyUnits * 100, 1))
                : 4;
        }

        metrics.MonthlyTrends = monthlyList;
        metrics.TotalUnitsSoldPeriod = monthlyList.Sum(x => x.ExitsUnits);
        metrics.TotalSalesValuePeriod = monthlyList.Sum(x => x.ExitsValue);

        // Ranking Top 6 Productos Más Vendidos / Mayor Rotación
        var exitDetails = periodMovements
            .Where(mov => mov.MovementType != null && (mov.MovementType.Mode == "S" || (mov.MovementType.Mode == "T" && mov.ReferenceSource != "REQUISICION_INTERNA_RECEPCION")))
            .SelectMany(mov => mov.Details)
            .Where(d => d.Product != null)
            .ToList();

        var topGrouping = exitDetails
            .GroupBy(d => d.IdProduct)
            .Select(g =>
            {
                var firstProd = g.First().Product!;
                int unitsSold = g.Sum(d => d.Quantity);
                decimal salesVal = g.Sum(d => d.Quantity * (d.UnitCost > 0 ? d.UnitCost : firstProd.SuggestedPrice));
                int currentStk = productStocks.TryGetValue(firstProd.IdProduct, out int stk) ? stk : 0;

                return new TopSellingProductDto
                {
                    IdProduct = firstProd.IdProduct,
                    ProductCode = firstProd.Code,
                    ProductName = firstProd.Name,
                    CategoryName = firstProd.Category?.Name ?? "General",
                    TotalUnitsSold = unitsSold,
                    TotalSalesValue = Math.Round(salesVal, 2),
                    CurrentStock = currentStk
                };
            })
            .OrderByDescending(x => x.TotalUnitsSold)
            .ThenByDescending(x => x.TotalSalesValue)
            .Take(6)
            .ToList();

        int maxProductSold = topGrouping.Select(x => x.TotalUnitsSold).DefaultIfEmpty(1).Max();
        if (maxProductSold <= 0) maxProductSold = 1;

        for (int idx = 0; idx < topGrouping.Count; idx++)
        {
            topGrouping[idx].Rank = idx + 1;
            topGrouping[idx].SharePercentage = Math.Max(12, Math.Round((double)topGrouping[idx].TotalUnitsSold / maxProductSold * 100, 1));
        }

        metrics.TopSellingProducts = topGrouping;

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

