using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventory_Flow.Services;

public class ExpirationAlertService : IExpirationAlertService
{
    private readonly AplicationDbContext _db;
    private readonly IN8nNotificationService _n8nService;
    private readonly IConfiguration _config;
    private readonly ILogger<ExpirationAlertService> _logger;

    public ExpirationAlertService(
        AplicationDbContext db,
        IN8nNotificationService n8nService,
        IConfiguration config,
        ILogger<ExpirationAlertService> logger)
    {
        _db = db;
        _n8nService = n8nService;
        _config = config;
        _logger = logger;
    }

    public async Task<ConsolidatedExpiringLotsReportDto> GetExpiringLotsReportAsync(int daysAhead = 30)
    {
        var now = DateTime.Today;
        var limitDate = now.AddDays(daysAhead);

        var rawLots = await _db.Lots
            .Include(l => l.Product)
                .ThenInclude(p => p!.Category)
            .Include(l => l.Warehouse)
            .AsNoTracking()
            .Where(l => l.IsActive && l.Quantity > 0 && l.ExpirationDate.Date <= limitDate)
            .OrderBy(l => l.IdWarehouse)
            .ThenBy(l => l.ExpirationDate)
            .ToListAsync();

        var report = new ConsolidatedExpiringLotsReportDto
        {
            GeneratedAt = DateTime.Now,
            DaysAhead = daysAhead
        };

        var groupedByWarehouse = rawLots.GroupBy(l => new { l.IdWarehouse, WarehouseName = l.Warehouse?.Name ?? $"Almacén #{l.IdWarehouse}" });

        foreach (var group in groupedByWarehouse)
        {
            var summary = new WarehouseExpiringLotsSummaryDto
            {
                WarehouseId = group.Key.IdWarehouse,
                WarehouseName = group.Key.WarehouseName
            };

            foreach (var l in group)
            {
                int remainingDays = (int)(l.ExpirationDate.Date - now).TotalDays;
                string urgency = remainingDays < 0 
                    ? "🚨 VENCIDO" 
                    : (remainingDays <= 7 ? "⚠️ CRÍTICO (<= 7 días)" : "⏰ POR VENCER (<= 30 días)");

                summary.ExpiringLots.Add(new ExpiringLotDto
                {
                    IdLot = l.IdLot,
                    LotNumber = l.LotNumber,
                    IdProduct = l.IdProduct,
                    ProductCode = l.Product?.Code ?? "-",
                    ProductName = l.Product?.Name ?? "Producto",
                    CategoryName = l.Product?.Category?.Name ?? "General",
                    IdWarehouse = group.Key.IdWarehouse,
                    WarehouseName = group.Key.WarehouseName,
                    Quantity = l.Quantity,
                    ExpirationDate = l.ExpirationDate,
                    DaysRemaining = remainingDays,
                    UrgencyLevel = urgency
                });
            }

            summary.TotalLotsAtRisk = summary.ExpiringLots.Count;
            summary.TotalUnitsAtRisk = summary.ExpiringLots.Sum(x => x.Quantity);

            // Formatear mensaje para la recepcionista de la sucursal
            var branchMsg = $"⏰ *ALERTA DE CADUCIDADES - {summary.WarehouseName.ToUpperInvariant()}*\n" +
                            $"Lotes próximos a caducar en los siguientes {daysAhead} días:\n\n";

            foreach (var item in summary.ExpiringLots)
            {
                branchMsg += $"• *{item.ProductName}* ({item.ProductCode})\n" +
                             $"  Lote: `{item.LotNumber}` | Cant: *{item.Quantity} uds*\n" +
                             $"  Vence: *{item.ExpirationDate:dd/MM/yyyy}* ({item.DaysRemaining} días restantes)\n" +
                             $"  Estado: {item.UrgencyLevel}\n\n";
            }
            branchMsg += $"🎯 _Acción recomendada:_ Priorizar venta inmediata según rotación PEPS/FIFO.";
            summary.FormattedWhatsAppMessage = branchMsg;

            report.Warehouses.Add(summary);
        }

        report.TotalWarehousesWithExpiringLots = report.Warehouses.Count;
        report.GrandTotalLotsAtRisk = report.Warehouses.Sum(w => w.TotalLotsAtRisk);
        report.GrandTotalUnitsAtRisk = report.Warehouses.Sum(w => w.TotalUnitsAtRisk);

        // Formatear resumen consolidado para Matriz
        var matrizMsg = $"📋 *REPORTE CONSOLIDADO DE CADUCIDADES (RED DE SUCURSALES)*\n" +
                        $"Fecha de análisis: {DateTime.Now:dd/MM/yyyy HH:mm}\n" +
                        $"Horizonte de tiempo: Próximos {daysAhead} días\n\n" +
                        $"Total Lotes en Riesgo: *{report.GrandTotalLotsAtRisk}*\n" +
                        $"Total Unidades en Riesgo: *{report.GrandTotalUnitsAtRisk} uds*\n\n";

        if (report.Warehouses.Count == 0)
        {
            matrizMsg += "✅ *Todo en orden:* No se registran lotes próximos a caducar dentro del umbral seleccionado.";
        }
        else
        {
            foreach (var wGroup in report.Warehouses)
            {
                matrizMsg += $"🏢 *{wGroup.WarehouseName}:* {wGroup.TotalLotsAtRisk} lotes ({wGroup.TotalUnitsAtRisk} uds)\n";
            }
            matrizMsg += $"\n💡 _Nota:_ Las recepcionistas de cada sucursal han recibido el desglose de sus lotes para acelerar rotación.";
        }

        report.MatrizConsolidatedWhatsAppMessage = matrizMsg;
        return report;
    }

    public async Task<N8nNotification> DispatchExpiringLotsWhatsAppAlertsAsync(int daysAhead = 30, string? matrizRecipient = null, int idUser = 1)
    {
        var report = await GetExpiringLotsReportAsync(daysAhead);
        var targetRecipient = !string.IsNullOrWhiteSpace(matrizRecipient)
            ? matrizRecipient
            : (_config["N8nSettings:MatrizRecipientPhone"] ?? "593963124396");

        var notification = await _n8nService.SendTextMessageAsync(
            targetRecipient,
            report.MatrizConsolidatedWhatsAppMessage,
            3, // Matriz Carapungo
            idUser
        );

        return notification;
    }
}
