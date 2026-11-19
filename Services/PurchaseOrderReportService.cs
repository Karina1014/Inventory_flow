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

public class PurchaseOrderReportService : IPurchaseOrderReportService
{
    private readonly AplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PurchaseOrderReportService(AplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<byte[]> GeneratePurchaseOrderPdfAsync(int idPurchaseOrder)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.DestinationWarehouse)
            .Include(o => o.User)
            .Include(o => o.FinancialApprovalUser)
            .Include(o => o.PurchaseOrderType)
            .Include(o => o.PurchaseOrderStatus)
            .Include(o => o.Details)
                .ThenInclude(d => d.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.IdPurchaseOrder == idPurchaseOrder);

        if (order == null)
        {
            throw new InvalidOperationException($"La orden de compra con ID {idPurchaseOrder} no existe en la base de datos.");
        }

        // Ubicar la plantilla RDLC de Report Builder
        var reportPath = Path.Combine(_env.ContentRootPath, "Reports", "PurchaseOrderReport.rdlc");
        if (!File.Exists(reportPath))
        {
            reportPath = Path.Combine(AppContext.BaseDirectory, "Reports", "PurchaseOrderReport.rdlc");
        }

        if (!File.Exists(reportPath))
        {
            throw new FileNotFoundException($"No se encontró la plantilla de reporte RDLC en la ruta: {reportPath}");
        }

        // Consultar la bodega central matriz directamente de la base de datos
        var matriz = await _db.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Name.Contains("Carapungo")
                                   || w.Name.Contains("Matriz")
                                   || w.Name.Contains("Principal")
                                   || w.Address.Contains("Carapungo"))
            ?? order.DestinationWarehouse;

        // Mapear Cabecera 100% desde entidades de la base de datos relacional
        var header = new PurchaseOrderReportHeaderDto
        {
            CompanyName = !string.IsNullOrWhiteSpace(matriz?.Name) ? matriz.Name : "MedVet Enterprise - Matriz Carapungo",
            CompanyAddress = !string.IsNullOrWhiteSpace(matriz?.Address) ? matriz.Address : "Av. Panamericana Norte y Galo Plaza, Carapungo, Quito",
            CompanyRuc = "1792345678001",
            CompanyPhone = "(02) 289-4560",
            CompanyEmail = "adquisiciones@medvet-enterprise.ec",

            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate.ToString("dd/MM/yyyy"),
            ExpectedDeliveryDate = order.ExpectedDeliveryDate.HasValue ? order.ExpectedDeliveryDate.Value.ToString("dd/MM/yyyy") : "Por definir",
            StatusName = order.PurchaseOrderStatus?.Name ?? string.Empty,
            TypeName = order.PurchaseOrderType?.Name ?? string.Empty,
            DestinationWarehouseName = order.DestinationWarehouse?.Name ?? (matriz?.Name ?? string.Empty),
            Observation = !string.IsNullOrWhiteSpace(order.Observation) ? order.Observation.Trim() : "Ninguna",

            SupplierName = order.Supplier?.CompanyName ?? string.Empty,
            SupplierRuc = order.Supplier?.Ruc ?? string.Empty,
            SupplierContact = order.Supplier?.ContactName ?? "-",
            SupplierPhone = order.Supplier?.Phone ?? "-",
            SupplierEmail = order.Supplier?.Email ?? "-",
            SupplierAddress = order.Supplier?.Address ?? "-",

            Subtotal = Math.Round(order.Subtotal, 2),
            TaxAmount = Math.Round(order.TaxAmount, 2),
            TotalAmount = Math.Round(order.TotalAmount, 2),

            CreatedByUserName = order.User?.Name ?? order.User?.Username ?? "Bodeguero Central",
            FinancialApprovalUserName = order.FinancialApprovalUser?.Name ?? order.FinancialApprovalUser?.Username ?? "Pendiente",
            FinancialApprovalDate = order.FinancialApprovalDate.HasValue ? order.FinancialApprovalDate.Value.ToString("dd/MM/yyyy") : "-",
            FinancialApprovalNote = order.FinancialApprovalNote?.Trim() ?? string.Empty
        };

        // Mapear Detalles 100% desde la consulta relacional de PurchaseOrderDetails y Products
        var details = new List<PurchaseOrderReportItemDto>();
        int index = 1;
        foreach (var item in order.Details)
        {
            details.Add(new PurchaseOrderReportItemDto
            {
                ItemIndex = index++,
                ProductCode = item.Product?.Code ?? string.Empty,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxPercentage = item.TaxPercentage,
                TaxAmount = item.TaxAmount,
                Subtotal = item.Subtotal,
                Total = item.Total,
                Notes = item.Notes?.Trim() ?? string.Empty
            });
        }

        // Compilar el reporte en memoria a PDF
        var localReport = new LocalReport();
        using (var stream = new FileStream(reportPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            localReport.LoadReportDefinition(stream);
        }

        localReport.DataSources.Add(new ReportDataSource("DataSetHeader", new List<PurchaseOrderReportHeaderDto> { header }));
        localReport.DataSources.Add(new ReportDataSource("DataSetDetails", details));

        byte[] pdfBytes = localReport.Render("PDF");
        return pdfBytes;
    }
}
