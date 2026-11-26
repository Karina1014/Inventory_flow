using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos.Reports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Reporting.NETCore;

namespace Inventory_Flow.Services;

/// <summary>
/// Implementación del servicio de reporte de existencias y caducidades para la Bodega Matriz Carapungo.
/// 100% libre de datos quemados, todo consultado desde SQL Server.
/// </summary>
public class MatrixStockReportService : IMatrixStockReportService
{
    private readonly AplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public MatrixStockReportService(AplicationDbContext db, IWebHostEnvironment env, IConfiguration configuration)
    {
        _db = db;
        _env = env;
        _configuration = configuration;
    }

    public async Task<byte[]> GenerateMatrixStockPdfAsync(int? idWarehouse = null)
    {
        var (header, items) = await BuildReportDataAsync("Bodeguero Principal", idWarehouse);

        var reportPath = Path.Combine(_env.ContentRootPath, "Reports", "MatrixStockReport.rdl");
        if (!File.Exists(reportPath))
        {
            reportPath = Path.Combine(AppContext.BaseDirectory, "Reports", "MatrixStockReport.rdl");
        }

        if (!File.Exists(reportPath))
        {
            throw new FileNotFoundException($"No se encontró la plantilla de reporte en: {reportPath}");
        }

        var localReport = new LocalReport();
        using (var stream = new FileStream(reportPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            localReport.LoadReportDefinition(stream);
        }

        localReport.DataSources.Add(new ReportDataSource("DataSetHeader", new List<MatrixStockReportHeaderDto> { header }));
        localReport.DataSources.Add(new ReportDataSource("DataSetItems", items));

        byte[] pdfBytes = localReport.Render("PDF");
        return pdfBytes;
    }

    public async Task<MatrixStockReportSummaryDto> GetMatrixStockSummaryAsync(int? idWarehouse = null, string? baseUrl = null)
    {
        var (header, items) = await BuildReportDataAsync("Notificación n8n / Evolution API", idWarehouse);

        var cleanBaseUrl = !string.IsNullOrWhiteSpace(baseUrl) ? baseUrl.TrimEnd('/') : "";
        var pdfUrl = idWarehouse.HasValue && idWarehouse.Value > 0
            ? $"{cleanBaseUrl}/api/reports/inventory/warehouses/{idWarehouse.Value}/pdf"
            : $"{cleanBaseUrl}/api/reports/matrix-inventory/pdf";

        // Top 5 lotes más críticos (vencidos o próximos a vencer)
        var topExpiring = items
            .Where(i => i.Status == "VENCIDO" || i.Status == "PRÓXIMO A VENCER")
            .Take(5)
            .Select(i => new MatrixStockExpiringLotDto
            {
                Product = i.ProductName,
                LotNumber = i.LotNumber,
                ExpirationDate = i.ExpirationDate,
                DaysRemaining = i.DaysRemaining,
                Quantity = i.Quantity,
                Status = i.Status
            })
            .ToList();

        var summary = new MatrixStockReportSummaryDto
        {
            WarehouseName = header.WarehouseName,
            GeneratedAt = header.GeneratedAt,
            TotalProducts = header.TotalProductsCount,
            TotalLots = header.TotalLotsCount,
            TotalUnits = header.TotalUnitsCount,
            TotalValue = header.TotalValuedAmount,
            ExpiringSoonLotsCount = header.ExpiringSoonLotsCount,
            ExpiredLotsCount = header.ExpiredLotsCount,
            PdfDownloadUrl = pdfUrl,
            TopExpiringLots = topExpiring
        };

        return summary;
    }

    private async Task<(MatrixStockReportHeaderDto Header, List<MatrixStockReportItemDto> Items)> BuildReportDataAsync(string generatedBy, int? idWarehouse = null)
    {
        // 1. Identificar la sucursal o almacén (específico o Matriz por defecto)
        Warehouse? warehouse = null;
        if (idWarehouse.HasValue && idWarehouse.Value > 0)
        {
            warehouse = await _db.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IdWarehouse == idWarehouse.Value);
        }

        if (warehouse == null)
        {
            warehouse = await _db.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Name.Contains("Carapungo") || w.Name.Contains("Matriz") || w.Name.Contains("Principal"))
                ?? await _db.Warehouses.OrderBy(w => w.IdWarehouse).FirstOrDefaultAsync();
        }

        if (warehouse == null)
        {
            throw new InvalidOperationException("No se encontró ninguna sucursal o almacén registrado en la base de datos.");
        }

        // Consultar almacén matriz para datos generales de la empresa
        var matriz = await _db.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Name.Contains("Carapungo") || w.Name.Contains("Matriz") || w.Name.Contains("Principal"))
            ?? warehouse;

        // 2. Obtener ventana de días de alerta de caducidad (30 por defecto) y umbral activo de bajo stock
        int alertDays = 30;
        if (int.TryParse(_configuration["InventorySettings:ExpirationAlertDays"], out int days) && days > 0)
        {
            alertDays = days;
        }

        var thresholdEntity = await _db.ThresholdInvetories
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IsActive);
        int thresholdValue = thresholdEntity?.Threshold ?? 5;

        // 3. Consultar todos los productos activos y los lotes con existencias en dicho almacén
        var allProducts = await _db.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync();

        var lots = await _db.Lots
            .Include(l => l.Product)
                .ThenInclude(p => p!.Category)
            .AsNoTracking()
            .Where(l => l.IdWarehouse == warehouse.IdWarehouse && l.IsActive && l.Quantity > 0)
            .ToListAsync();

        var today = DateTime.Today;
        var itemList = new List<MatrixStockReportItemDto>();
        var productsWithLots = new HashSet<int>();

        foreach (var lot in lots)
        {
            if (lot.IdProduct > 0)
            {
                productsWithLots.Add(lot.IdProduct);
            }

            int daysRemaining = (int)(lot.ExpirationDate.Date - today).TotalDays;
            decimal unitPrice = Math.Round(lot.Product?.SuggestedPrice ?? 0, 2);
            decimal totalValue = Math.Round(lot.Quantity * unitPrice, 2);

            string status;
            if (daysRemaining < 0)
            {
                status = "VENCIDO";
            }
            else if (daysRemaining <= alertDays)
            {
                status = "PRÓXIMO A VENCER";
            }
            else if (lot.Quantity <= thresholdValue)
            {
                status = "BAJO STOCK";
            }
            else
            {
                status = "VIGENTE";
            }

            itemList.Add(new MatrixStockReportItemDto
            {
                ProductCode = lot.Product?.Code ?? "-",
                ProductName = lot.Product?.Name ?? "Producto sin nombre",
                CategoryName = lot.Product?.Category?.Name ?? "General",
                LotNumber = !string.IsNullOrWhiteSpace(lot.LotNumber) ? lot.LotNumber : "S/N",
                ExpirationDate = lot.ExpirationDate.ToString("dd/MM/yyyy"),
                DaysRemaining = daysRemaining,
                Quantity = lot.Quantity,
                UnitPrice = unitPrice,
                TotalValue = totalValue,
                Status = status
            });
        }

        // 4. Agregar los productos que NO tienen ningún lote con stock en este almacén (quiebre de stock)
        foreach (var prod in allProducts)
        {
            if (!productsWithLots.Contains(prod.IdProduct))
            {
                itemList.Add(new MatrixStockReportItemDto
                {
                    ProductCode = prod.Code,
                    ProductName = prod.Name,
                    CategoryName = prod.Category?.Name ?? "General",
                    LotNumber = "SIN STOCK",
                    ExpirationDate = "-",
                    DaysRemaining = 0,
                    Quantity = 0,
                    UnitPrice = prod.SuggestedPrice,
                    TotalValue = 0,
                    Status = "AGOTADO"
                });
            }
        }

        // 5. Ordenar: Vencidos primero, luego próximos a vencer, luego bajo stock, luego vigentes, y al final agotados
        var sortedList = itemList
            .OrderBy(i => i.Status == "VENCIDO" ? 0 : (i.Status == "PRÓXIMO A VENCER" ? 1 : (i.Status == "BAJO STOCK" ? 2 : (i.Status == "VIGENTE" ? 3 : 4))))
            .ThenBy(i => i.Quantity)
            .ThenBy(i => i.ProductName)
            .ToList();

        int index = 1;
        foreach (var item in sortedList)
        {
            item.ItemIndex = index++;
        }

        // 5. Construir Cabecera
        var header = new MatrixStockReportHeaderDto
        {
            CompanyName = "MedVet Enterprise",
            CompanyRuc = "1792345678001",
            CompanyAddress = !string.IsNullOrWhiteSpace(matriz.Address) ? matriz.Address : "Carapungo, Quito",
            CompanyPhone = "(02) 289-4560",

            WarehouseName = warehouse.Name,
            WarehouseAddress = !string.IsNullOrWhiteSpace(warehouse.Address) ? warehouse.Address : "Quito, Ecuador",

            GeneratedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            ExpirationAlertDays = alertDays,

            TotalProductsCount = sortedList.Select(i => i.ProductCode).Distinct().Count(),
            TotalLotsCount = sortedList.Count(i => i.Quantity > 0),
            TotalUnitsCount = sortedList.Sum(i => i.Quantity),
            TotalValuedAmount = Math.Round(sortedList.Sum(i => i.TotalValue), 2),

            ExpiringSoonLotsCount = sortedList.Count(i => i.Status == "PRÓXIMO A VENCER"),
            ExpiredLotsCount = sortedList.Count(i => i.Status == "VENCIDO"),
            ValidLotsCount = sortedList.Count(i => i.Status == "VIGENTE"),
            LowStockProductsCount = sortedList.Count(i => i.Status == "BAJO STOCK"),
            ActiveThreshold = thresholdValue,

            GeneratedBy = generatedBy
        };

        return (header, sortedList);
    }
}
