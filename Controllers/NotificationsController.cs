using System;
using System.IO;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Inventory_Flow.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly ILowStockReportService _reportService;
    private readonly AplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public NotificationsController(
        ILowStockReportService reportService,
        AplicationDbContext db,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _reportService = reportService;
        _db = db;
        _configuration = configuration;
        _env = env;
    }

    /// <summary>
    /// Endpoint para n8n: Genera el PDF de alerta de stock, registra en N8nNotifications (Pending)
    /// y retorna los datos listos para el nodo de Evolution API (WhatsApp).
    /// </summary>
    [HttpPost("dispatch-stock-alert")]
    [HttpGet("dispatch-stock-alert")]
    public async Task<IActionResult> DispatchStockAlert(
        [FromQuery] int warehouseId = 3,
        [FromQuery] string recipient = "593963124396",
        [FromQuery] string? customMessage = null)
    {
        try
        {
            // 1. Obtener la bodega
            var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.IdWarehouse == warehouseId);
            var warehouseName = warehouse?.Name ?? "Medvet Principal";

            // 2. Generar el PDF oficial con el motor RDL
            var pdfBytes = await _reportService.GenerateLowStockPdfByWarehouseAsync(warehouseId);
            var fileName = $"Alerta_Stock_{warehouseName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            // 3. Guardar archivo local en wwwroot/reports/stock_alerts/
            var reportsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "reports", "stock_alerts");
            if (!Directory.Exists(reportsFolder))
            {
                Directory.CreateDirectory(reportsFolder);
            }
            var fullPath = Path.Combine(reportsFolder, fileName);
            await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

            var relativePath = $"reports/stock_alerts/{fileName}";
            var pdfBase64 = Convert.ToBase64String(pdfBytes);

            var messageText = !string.IsNullOrWhiteSpace(customMessage)
                ? customMessage
                : $"⚠️ *Alerta de Stock Bajo - {warehouseName}*\nSe adjunta el reporte oficial de existencias y productos bajo umbral para revisión inmediata.";

            // 4. Crear registro en N8nNotifications (Pending)
            var notification = new N8nNotification
            {
                IdN8nNotificationType = 2, // PDF
                IdWarehouse = warehouseId,
                IdUser = 1,
                Recipient = recipient,
                Message = messageText,
                PdfPath = relativePath,
                Status = "Pending",
                Response = "Generado por .NET, entregado a n8n para Evolution API",
                SentDate = DateTime.Now
            };

            _db.N8nNotifications.Add(notification);
            await _db.SaveChangesAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var mediaUrl = $"{baseUrl}/{relativePath}";

            // 5. Retornar payload para n8n
            return Ok(new
            {
                idNotification = notification.IdN8nNotification,
                recipient = recipient,
                caption = messageText,
                fileName = fileName,
                mediaUrl = mediaUrl,
                base64 = pdfBase64,
                warehouseId = warehouseId,
                warehouseName = warehouseName,
                sentAt = notification.SentDate.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            var fullError = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return BadRequest(new { error = $"Error generando la alerta de stock: {fullError}" });
        }
    }
}
