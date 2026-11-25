using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Inventory_Flow.Models;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventory_Flow.Services;

public class N8nNotificationService : IN8nNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IN8nNotificationRepository _repo;
    private readonly ILowStockReportService _lowStockReportService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<N8nNotificationService> _logger;

    public N8nNotificationService(
        HttpClient httpClient,
        IConfiguration config,
        IN8nNotificationRepository repo,
        ILowStockReportService lowStockReportService,
        IWebHostEnvironment env,
        ILogger<N8nNotificationService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _repo = repo;
        _lowStockReportService = lowStockReportService;
        _env = env;
        _logger = logger;
    }

    public async Task<N8nNotification> SendTextMessageAsync(string recipient, string message, int idWarehouse = 3, int idUser = 1)
    {
        var notification = new N8nNotification
        {
            IdN8nNotificationType = 1, // Text
            IdWarehouse = idWarehouse,
            IdUser = idUser,
            Recipient = recipient,
            Message = message,
            PdfPath = string.Empty,
            Status = "Pending",
            Response = string.Empty,
            SentDate = DateTime.Now
        };

        await _repo.CreateAsync(notification);

        var webhookUrl = _config["N8nSettings:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            notification.Status = "Failed";
            notification.Response = "N8nSettings:WebhookUrl no está configurada en appsettings.json";
            await _repo.UpdateStatusAsync(notification.IdN8nNotification, notification.Status, notification.Response);
            return notification;
        }

        try
        {
            var payload = new
            {
                type = "Text",
                recipient = recipient,
                message = message,
                warehouseId = idWarehouse,
                notificationId = notification.IdN8nNotification,
                sentAt = notification.SentDate.ToString("o")
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var apiKey = _config["N8nSettings:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                jsonContent.Headers.TryAddWithoutValidation("X-API-KEY", apiKey);
            }

            var httpResponse = await _httpClient.PostAsync(webhookUrl, jsonContent);
            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)
            {
                notification.Status = "Sent";
                notification.Response = string.IsNullOrWhiteSpace(responseBody) ? $"HTTP {(int)httpResponse.StatusCode} OK" : responseBody;
            }
            else
            {
                notification.Status = "Failed";
                notification.IdN8nNotificationType = 3; // Error
                notification.Response = $"HTTP {(int)httpResponse.StatusCode}: {responseBody}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje de texto a n8n");
            notification.Status = "Failed";
            notification.IdN8nNotificationType = 3; // Error
            notification.Response = $"Excepción: {ex.Message}";
        }

        await _repo.UpdateStatusAsync(notification.IdN8nNotification, notification.Status, notification.Response);
        return notification;
    }

    public async Task<N8nNotification> SendStockAlertPdfAsync(string recipient, string message, int idWarehouse, int idUser = 1)
    {
        string relativePdfPath = string.Empty;
        string pdfBase64 = string.Empty;
        string fileName = $"AlertaStock_Bodega_{idWarehouse}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

        try
        {
            // 1. Generar el PDF oficial de stock bajo con el servicio existente
            var pdfBytes = await _lowStockReportService.GenerateLowStockPdfByWarehouseAsync(idWarehouse);

            // 2. Guardar archivo local en wwwroot/reports/stock_alerts/
            var reportsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "reports", "stock_alerts");
            if (!Directory.Exists(reportsFolder))
            {
                Directory.CreateDirectory(reportsFolder);
            }

            var fullPath = Path.Combine(reportsFolder, fileName);
            await File.WriteAllBytesAsync(fullPath, pdfBytes);

            relativePdfPath = $"reports/stock_alerts/{fileName}";
            pdfBase64 = Convert.ToBase64String(pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar el PDF de alerta de stock para la bodega {WarehouseId}", idWarehouse);
        }

        var notification = new N8nNotification
        {
            IdN8nNotificationType = 2, // PDF
            IdWarehouse = idWarehouse,
            IdUser = idUser,
            Recipient = recipient,
            Message = message,
            PdfPath = relativePdfPath,
            Status = "Pending",
            Response = string.Empty,
            SentDate = DateTime.Now
        };

        await _repo.CreateAsync(notification);

        var webhookUrl = _config["N8nSettings:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            notification.Status = "Failed";
            notification.Response = "N8nSettings:WebhookUrl no está configurada en appsettings.json";
            await _repo.UpdateStatusAsync(notification.IdN8nNotification, notification.Status, notification.Response);
            return notification;
        }

        try
        {
            var payload = new
            {
                type = "PDF",
                category = "StockAlert",
                recipient = recipient,
                message = message,
                warehouseId = idWarehouse,
                fileName = fileName,
                fileUrl = relativePdfPath,
                pdfBase64 = pdfBase64,
                notificationId = notification.IdN8nNotification,
                sentAt = notification.SentDate.ToString("o")
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var apiKey = _config["N8nSettings:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                jsonContent.Headers.TryAddWithoutValidation("X-API-KEY", apiKey);
            }

            var httpResponse = await _httpClient.PostAsync(webhookUrl, jsonContent);
            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)
            {
                notification.Status = "Sent";
                notification.Response = string.IsNullOrWhiteSpace(responseBody) ? $"HTTP {(int)httpResponse.StatusCode} OK" : responseBody;
            }
            else
            {
                notification.Status = "Failed";
                notification.IdN8nNotificationType = 3; // Error
                notification.Response = $"HTTP {(int)httpResponse.StatusCode}: {responseBody}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar PDF de alerta de stock a n8n");
            notification.Status = "Failed";
            notification.IdN8nNotificationType = 3; // Error
            notification.Response = $"Excepción: {ex.Message}";
        }

        await _repo.UpdateStatusAsync(notification.IdN8nNotification, notification.Status, notification.Response);
        return notification;
    }

    public async Task<N8nNotification> SendLowStockSaleAlertAsync(
        int warehouseId,
        string warehouseName,
        int productId,
        string productCode,
        string productName,
        int remainingStock,
        int threshold,
        string recipient,
        int idUser = 1)
    {
        string statusHeader = remainingStock == 0 ? "🚨 *¡ALERTA DE QUIEBRE DE STOCK (AGOTADO)!*" : "⚠️ *ALERTA DE STOCK BAJO (POR AGOTARSE)*";
        string statusTag = remainingStock == 0 ? "AGOTADO" : "BAJO STOCK";

        string message = $"{statusHeader}\n\n" +
                         $"🏢 *Sucursal:* {warehouseName}\n" +
                         $"📦 *Producto:* {productName} ({productCode})\n" +
                         $"📉 *Stock Restante:* {remainingStock} unidad(es)\n" +
                         $"🎯 *Umbral Mínimo:* {threshold} unidades\n\n" +
                         $"🔔 *Acción requerida:* Se requiere preparar reposición/transferencia desde Matriz Carapungo hacia {warehouseName}.";

        var notification = new N8nNotification
        {
            IdN8nNotificationType = 1, // Text
            IdWarehouse = warehouseId,
            IdUser = idUser,
            Recipient = recipient,
            Message = message,
            PdfPath = string.Empty,
            Status = "Pending",
            Response = string.Empty,
            SentDate = DateTime.Now
        };

        await _repo.CreateAsync(notification);

        var webhookUrl = _config["N8nSettings:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            notification.Status = "Failed";
            notification.Response = "N8nSettings:WebhookUrl no configurada en appsettings.json";
            await _repo.UpdateStatusAsync(notification.IdN8nNotification, notification.Status, notification.Response);
            return notification;
        }

        try
        {
            var payload = new
            {
                @event = "SaleLowStockAlert",
                type = "Text",
                recipient = recipient,
                warehouseId = warehouseId,
                warehouseName = warehouseName,
                productId = productId,
                productCode = productCode,
                productName = productName,
                remainingStock = remainingStock,
                threshold = threshold,
                status = statusTag,
                message = message,
                notificationId = notification.IdN8nNotification,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var apiKey = _config["N8nSettings:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                jsonContent.Headers.TryAddWithoutValidation("X-API-KEY", apiKey);
            }

            var httpResponse = await _httpClient.PostAsync(webhookUrl, jsonContent);
            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)
            {
                notification.Status = "Sent";
                notification.Response = string.IsNullOrWhiteSpace(responseBody) ? $"HTTP {(int)httpResponse.StatusCode} OK" : responseBody;
            }
            else
            {
                notification.Status = "Failed";
                notification.IdN8nNotificationType = 3; // Error
                notification.Response = $"HTTP {(int)httpResponse.StatusCode}: {responseBody}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar webhook de alerta de venta con stock bajo a n8n");
            notification.Status = "Failed";
            notification.IdN8nNotificationType = 3; // Error
            notification.Response = $"Excepción: {ex.Message}";
        }

        await _repo.UpdateStatusAsync(notification.IdN8nNotification, notification.Status, notification.Response);
        return notification;
    }
}
