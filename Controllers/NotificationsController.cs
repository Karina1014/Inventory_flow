using System;
using System.IO;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Repository.IRepository;
using Inventory_Flow.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Inventory_Flow.Controllers;

public class UpdateNotificationStatusRequest
{
    public int IdNotification { get; set; }
    public string Status { get; set; } = "Sent";
    public string? Response { get; set; }
}

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly ILowStockReportService _reportService;
    private readonly IN8nNotificationService _n8nNotificationService;
    private readonly IExpirationAlertService _expirationAlertService;
    private readonly IN8nNotificationRepository _notificationRepo;
    private readonly INotificationRecipientService _recipientService;
    private readonly AplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public NotificationsController(
        ILowStockReportService reportService,
        IN8nNotificationService n8nNotificationService,
        IExpirationAlertService expirationAlertService,
        IN8nNotificationRepository notificationRepo,
        INotificationRecipientService recipientService,
        AplicationDbContext db,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _reportService = reportService;
        _n8nNotificationService = n8nNotificationService;
        _expirationAlertService = expirationAlertService;
        _notificationRepo = notificationRepo;
        _recipientService = recipientService;
        _db = db;
        _configuration = configuration;
        _env = env;
    }

    // Endpoint para n8n: Genera el PDF de alerta de stock, registra en N8nNotifications (Pending)
    // y retorna los datos listos para el nodo de Evolution API (WhatsApp).
    [HttpPost("dispatch-stock-alert")]
    [HttpGet("dispatch-stock-alert")]
    public async Task<IActionResult> DispatchStockAlert(
        [FromQuery] int warehouseId = 3,
        [FromQuery] string? recipient = null,
        [FromQuery] string? customMessage = null)
    {
        try
        {
            // Resolver destinatario según rol en base de datos si no se envió explícito
            var targetRecipient = !string.IsNullOrWhiteSpace(recipient)
                ? recipient
                : await _recipientService.GetBodegueroPrincipalPhoneAsync();

            // 1. Obtener la bodega
            var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.IdWarehouse == warehouseId);
            var warehouseName = warehouse?.Name ?? "Medvet Principal";

            // 2. Generar el PDF oficial en memoria con el motor RDL (sin guardar en disco)
            var pdfBytes = await _reportService.GenerateLowStockPdfByWarehouseAsync(warehouseId);
            var fileName = $"Alerta_Stock_{warehouseName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            var pdfBase64 = Convert.ToBase64String(pdfBytes);

            var messageText = !string.IsNullOrWhiteSpace(customMessage)
                ? customMessage
                : $"⚠️ *Alerta de Stock Bajo - {warehouseName}*\nSe adjunta el reporte oficial de existencias y productos bajo umbral para revisión inmediata.";

            // 3. Crear registro en N8nNotifications (Pending)
            var notification = new N8nNotification
            {
                IdN8nNotificationType = 2, // PDF
                IdWarehouse = warehouseId,
                IdUser = 1,
                Recipient = targetRecipient,
                Message = messageText,
                PdfPath = string.Empty, // 100% en memoria, cero consumo de disco
                Status = "Pending",
                Response = "Generado por .NET, entregado a n8n para Evolution API",
                SentDate = DateTime.Now
            };

            _db.N8nNotifications.Add(notification);
            await _db.SaveChangesAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var mediaUrl = $"{baseUrl}/api/notifications/{notification.IdN8nNotification}/pdf";

            // 4. Retornar payload para n8n con Base64 y URL de streaming en memoria
            return Ok(new
            {
                idNotification = notification.IdN8nNotification,
                recipient = targetRecipient,
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

    // Simula una alerta en tiempo real de venta con stock bajo/agotado enviada a n8n por Webhook
    [HttpPost("test-sale-alert")]
    [HttpGet("test-sale-alert")]
    public async Task<IActionResult> TestSaleAlert(
        [FromQuery] int warehouseId = 4,
        [FromQuery] int remainingStock = 2,
        [FromQuery] string? recipient = null)
    {
        try
        {
            var targetRecipient = !string.IsNullOrWhiteSpace(recipient)
                ? recipient
                : await _recipientService.GetBodegueroPrincipalPhoneAsync();

            var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.IdWarehouse == warehouseId);
            var warehouseName = warehouse?.Name ?? "Llano Grande";

            var product = await _db.Products.FirstOrDefaultAsync(p => p.IsActive);
            var prodName = product?.Name ?? "Balanceado Cachorro";
            var prodCode = product?.Code ?? "MED-001";
            var prodId = product?.IdProduct ?? 1;

            var notif = await _n8nNotificationService.SendLowStockSaleAlertAsync(
                warehouseId,
                warehouseName,
                prodId,
                prodCode,
                prodName,
                remainingStock,
                5,
                targetRecipient,
                1
            );

            return Ok(notif);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Consulta los lotes próximos a vencer agrupados por sucursal dentro del rango especificado (días).
    // Endpoint ideal para el nodo HTTP Request de n8n en el flujo programado matutino.
    [HttpGet("expiring-lots")]
    public async Task<IActionResult> GetExpiringLots([FromQuery] int daysAhead = 30)
    {
        try
        {
            var report = await _expirationAlertService.GetExpiringLotsReportAsync(daysAhead);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Error al consultar lotes próximos a vencer: {ex.Message}" });
        }
    }

    // Dispara y despacha la alerta de caducidades a n8n / WhatsApp (Bodega Matriz y Sucursales)
    [HttpPost("dispatch-expiring-lots")]
    [HttpGet("dispatch-expiring-lots")]
    public async Task<IActionResult> DispatchExpiringLots(
        [FromQuery] int daysAhead = 30,
        [FromQuery] string? recipient = null)
    {
        try
        {
            var targetRecipient = !string.IsNullOrWhiteSpace(recipient)
                ? recipient
                : await _recipientService.GetBodegueroPrincipalPhoneAsync();

            var notif = await _expirationAlertService.DispatchExpiringLotsWhatsAppAlertsAsync(daysAhead, targetRecipient, 1);
            return Ok(notif);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Error al despachar alerta de caducidades: {ex.Message}" });
        }
    }

    // Consulta los destinatarios resueltos por rol y sucursal desde la base de datos
    [HttpGet("recipients")]
    public async Task<IActionResult> GetRecipients()
    {
        var recipients = await _recipientService.GetRecipientsSummaryAsync();
        var bodegueroPrincipal = await _recipientService.GetBodegueroPrincipalPhoneAsync();
        return Ok(new
        {
            bodegueroPrincipalMatrizPhone = bodegueroPrincipal,
            usersWithPhoneInDb = recipients
        });
    }

    // Endpoint para que n8n actualice el estado de la notificación a 'Sent' o 'Failed' tras enviar por WhatsApp
    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateNotificationStatus([FromBody] UpdateNotificationStatusRequest request)
    {
        if (request == null || request.IdNotification <= 0)
        {
            return BadRequest(new { error = "El idNotification es requerido." });
        }

        var status = !string.IsNullOrWhiteSpace(request.Status) ? request.Status : "Sent";
        var responseText = !string.IsNullOrWhiteSpace(request.Response) 
            ? request.Response 
            : "Mensaje entregado exitosamente por n8n / Evolution API";

        var updated = await _notificationRepo.UpdateStatusAsync(request.IdNotification, status, responseText);
        if (!updated)
        {
            return NotFound(new { error = $"No se encontró la notificación con ID {request.IdNotification}" });
        }

        return Ok(new
        {
            success = true,
            idNotification = request.IdNotification,
            status = status,
            updatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    // Sobrecarga GET para pruebas rápidas desde navegador o llamadas HTTP simples
    [HttpGet("update-status")]
    public async Task<IActionResult> UpdateNotificationStatusGet(
        [FromQuery] int idNotification,
        [FromQuery] string status = "Sent",
        [FromQuery] string response = "Mensaje entregado exitosamente por n8n / Evolution API")
    {
        return await UpdateNotificationStatus(new UpdateNotificationStatusRequest
        {
            IdNotification = idNotification,
            Status = status,
            Response = response
        });
    }

    // Descarga o visualiza el PDF de la notificación generado 100% en memoria (Streaming HTTP sin ocupar disco)
    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> GetNotificationPdf(int id)
    {
        var notification = await _notificationRepo.GetByIdAsync(id);
        if (notification == null)
        {
            return NotFound($"Notificación #{id} no encontrada.");
        }

        try
        {
            // Generar el PDF dinámicamente en memoria según la sucursal de la notificación
            var pdfBytes = await _reportService.GenerateLowStockPdfByWarehouseAsync(notification.IdWarehouse);
            var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.IdWarehouse == notification.IdWarehouse);
            var warehouseName = warehouse?.Name ?? $"Sucursal_{notification.IdWarehouse}";
            var fileName = $"Alerta_Stock_{warehouseName.Replace(" ", "_")}_{notification.SentDate:yyyyMMdd_HHmm}.pdf";

            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");
            return File(pdfBytes, "application/pdf");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al renderizar el reporte PDF en memoria: {ex.Message}");
        }
    }
}
