using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models.Dtos.Reports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.NETCore;

namespace Inventory_Flow.Services;

public class LowStockReportService : ILowStockReportService
{
    private readonly AplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public LowStockReportService(AplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<byte[]> GenerateLowStockPdfByWarehouseAsync(int idWarehouse)
    {
        var (header, items) = await BuildReportDataAsync(idWarehouse, "Servicio Automático n8n / Evolution API");

        // Ubicar la plantilla RDLC
        var reportPath = Path.Combine(_env.ContentRootPath, "Reports", "LowStockWarehouseReport.rdlc");
        if (!File.Exists(reportPath))
        {
            reportPath = Path.Combine(AppContext.BaseDirectory, "Reports", "LowStockWarehouseReport.rdlc");
        }

        if (!File.Exists(reportPath))
        {
            throw new FileNotFoundException($"No se encontró la plantilla de reporte RDLC en: {reportPath}");
        }

        // Renderizar con ReportViewerCore
        var localReport = new LocalReport();
        using (var stream = new FileStream(reportPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            localReport.LoadReportDefinition(stream);
        }

        localReport.DataSources.Add(new ReportDataSource("DataSetHeader", new List<LowStockReportHeaderDto> { header }));
        localReport.DataSources.Add(new ReportDataSource("DataSetItems", items));

        byte[] pdfBytes = localReport.Render("PDF");
        return pdfBytes;
    }

    public async Task<LowStockReportSummaryDto> GetLowStockSummaryByWarehouseAsync(int idWarehouse, string? baseUrl = null)
    {
        var (header, items) = await BuildReportDataAsync(idWarehouse, "Consulta API n8n");

        var cleanBaseUrl = !string.IsNullOrWhiteSpace(baseUrl) ? baseUrl.TrimEnd('/') : "";
        var pdfUrl = $"{cleanBaseUrl}/api/reports/low-stock/warehouses/{idWarehouse}/pdf";

        var summary = new LowStockReportSummaryDto
        {
            WarehouseId = header.WarehouseId,
            WarehouseName = header.WarehouseName,
            WarehouseAddress = header.WarehouseAddress,
            ActiveThreshold = header.ActiveThreshold,
            TotalAlerts = header.TotalAlertCount,
            CriticalCount = header.TotalCriticalCount,
            WarningCount = header.TotalWarningCount,
            GeneratedAt = header.GeneratedAt,
            PdfDownloadUrl = pdfUrl,
            CriticalItems = items.Select(i => new LowStockAlertItemDto
            {
                Code = i.ProductCode,
                Product = i.ProductName,
                Category = i.CategoryName,
                CurrentStock = i.CurrentStock,
                Threshold = i.Threshold,
                Status = i.AlertLevel
            }).ToList()
        };

        return summary;
    }

    private async Task<(LowStockReportHeaderDto Header, List<LowStockReportItemDto> Items)> BuildReportDataAsync(int idWarehouse, string generatedBy)
    {
        // 1. Validar existencia del almacén/sucursal
        var warehouse = await _db.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.IdWarehouse == idWarehouse);

        if (warehouse == null)
        {
            throw new InvalidOperationException($"La sucursal o almacén con ID {idWarehouse} no existe en la base de datos.");
        }

        // 2. Consultar almacén matriz para datos de la empresa
        var matriz = await _db.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Name.Contains("Carapungo") || w.Name.Contains("Matriz"))
            ?? warehouse;

        // 3. Obtener el umbral activo del inventario
        var thresholdEntity = await _db.ThresholdInvetories
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IsActive);

        int thresholdValue = thresholdEntity?.Threshold ?? 5;

        // 4. Consultar lotes con existencias en dicha sucursal
        var lots = await _db.Lots
            .AsNoTracking()
            .Where(l => l.IdWarehouse == idWarehouse && l.IsActive && l.Quantity > 0)
            .ToListAsync();

        var stockByProduct = lots
            .GroupBy(l => l.IdProduct)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

        var nearestExpByProduct = lots
            .GroupBy(l => l.IdProduct)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.ExpirationDate).First().ExpirationDate);

        // 5. Consultar todos los productos activos de la empresa
        var products = await _db.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync();

        // 6. Filtrar productos que están bajo stock (stock <= umbral)
        var alertList = new List<LowStockReportItemDto>();

        foreach (var prod in products)
        {
            int currentStock = stockByProduct.TryGetValue(prod.IdProduct, out int qty) ? qty : 0;

            if (currentStock <= thresholdValue)
            {
                int deficit = Math.Max(1, thresholdValue - currentStock);
                string expDateStr = nearestExpByProduct.TryGetValue(prod.IdProduct, out var expDate)
                    ? expDate.ToString("dd/MM/yyyy")
                    : "-";

                alertList.Add(new LowStockReportItemDto
                {
                    ProductCode = prod.Code,
                    ProductName = prod.Name,
                    CategoryName = prod.Category?.Name ?? "General",
                    CurrentStock = currentStock,
                    Threshold = thresholdValue,
                    DeficitQuantity = deficit,
                    SuggestedPrice = prod.SuggestedPrice,
                    AlertLevel = currentStock == 0 ? "AGOTADO" : "BAJO STOCK",
                    NearestExpirationDate = expDateStr
                });
            }
        }

        // 7. Ordenar: primero quiebre total (0), luego menor stock a mayor stock, y luego nombre
        var sortedList = alertList
            .OrderBy(i => i.CurrentStock)
            .ThenBy(i => i.ProductName)
            .ToList();

        // Numerar secuencialmente
        int index = 1;
        foreach (var item in sortedList)
        {
            item.ItemIndex = index++;
        }

        // 8. Construir DTO de cabecera
        var header = new LowStockReportHeaderDto
        {
            CompanyName = !string.IsNullOrWhiteSpace(matriz?.Name) ? matriz.Name : "MedVet Enterprise",
            CompanyRuc = "1792345678001",
            CompanyAddress = !string.IsNullOrWhiteSpace(matriz?.Address) ? matriz.Address : "Av. Panamericana Norte y Galo Plaza, Carapungo, Quito",
            CompanyPhone = "(02) 289-4560",

            WarehouseId = warehouse.IdWarehouse,
            WarehouseName = warehouse.Name,
            WarehouseAddress = !string.IsNullOrWhiteSpace(warehouse.Address) ? warehouse.Address : "Sin dirección registrada",

            GeneratedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            ActiveThreshold = thresholdValue,

            TotalCriticalCount = sortedList.Count(i => i.CurrentStock == 0),
            TotalWarningCount = sortedList.Count(i => i.CurrentStock > 0),
            TotalAlertCount = sortedList.Count,

            GeneratedBy = generatedBy
        };

        return (header, sortedList);
    }
}
