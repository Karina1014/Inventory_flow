using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventory_Flow.Services;

public class FinancialAiAssistantService : IFinancialAiAssistantService
{
    private readonly AplicationDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FinancialAiAssistantService> _logger;

    public FinancialAiAssistantService(
        AplicationDbContext db,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<FinancialAiAssistantService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<PurchaseOrder>> GetPendingOrdersForAnalysisAsync()
    {
        return await _db.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.DestinationWarehouse)
            .Include(o => o.PurchaseOrderType)
            .Include(o => o.PurchaseOrderStatus)
            .Include(o => o.Details)
                .ThenInclude(d => d.Product)
            .Where(o => o.IdPurchaseOrderStatus == 2)
            .OrderByDescending(o => o.OrderDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<FinancialOrderAnalysisDto> AnalyzePurchaseOrderAsync(int orderId)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.DestinationWarehouse)
            .Include(o => o.PurchaseOrderType)
            .Include(o => o.Details)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p!.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.IdPurchaseOrder == orderId);

        if (order == null)
        {
            throw new InvalidOperationException($"No se encontró la orden de compra con ID {orderId}.");
        }

        var thresholdEntity = await _db.ThresholdInvetories.AsNoTracking().FirstOrDefaultAsync(t => t.IsActive);
        int activeThreshold = thresholdEntity?.Threshold ?? 5;

        var result = new FinancialOrderAnalysisDto
        {
            OrderId = order.IdPurchaseOrder,
            OrderNumber = order.OrderNumber,
            SupplierName = order.Supplier?.CompanyName ?? "Proveedor no asignado",
            OrderType = order.PurchaseOrderType?.Name ?? "Estándar",
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            TotalItemsCount = order.Details.Count,
            AnalyzedAt = DateTime.Now
        };

        decimal totalPotentialOvercost = 0;
        decimal totalVariancePct = 0;
        int priceAlerts = 0;
        int quantityAlerts = 0;

        foreach (var detail in order.Details)
        {
            var product = detail.Product;
            int productId = detail.IdProduct;

            int currentStock = await _db.Lots
                .Where(l => l.IdProduct == productId && l.IdWarehouse == order.IdDestinationWarehouse && l.IsActive && l.Quantity > 0)
                .SumAsync(l => (int?)l.Quantity) ?? 0;

            var pastDetails = await _db.PurchaseOrderDetails
                .Include(pod => pod.PurchaseOrder)
                .Where(pod => pod.IdProduct == productId && pod.IdPurchaseOrder != order.IdPurchaseOrder && pod.UnitPrice > 0)
                .OrderByDescending(pod => pod.PurchaseOrder!.OrderDate)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            decimal? lastPrice = pastDetails.FirstOrDefault()?.UnitPrice ?? product?.SuggestedPrice;
            decimal? avgHistoricalPrice = pastDetails.Any() ? pastDetails.Average(p => p.UnitPrice) : product?.SuggestedPrice;

            decimal priceVariance = 0;
            if (lastPrice.HasValue && lastPrice.Value > 0)
            {
                priceVariance = Math.Round(((detail.UnitPrice - lastPrice.Value) / lastPrice.Value) * 100m, 1);
            }

            var sixtyDaysAgo = DateTime.Today.AddDays(-60);
            var movementsOutCount = await _db.InventoryMovementDetails
                .Include(imd => imd.InventoryMovement)
                .Where(imd => imd.IdProduct == productId && imd.InventoryMovement!.MovementDate >= sixtyDaysAgo && imd.InventoryMovement.IdOriginWarehouse == order.IdDestinationWarehouse)
                .SumAsync(imd => (int?)imd.Quantity) ?? 0;

            int estimatedMonthlyDemand = Math.Max((int)Math.Round(movementsOutCount / 2.0), Math.Max(activeThreshold, 8));
            decimal monthsCoverage = estimatedMonthlyDemand > 0 ? Math.Round((decimal)(currentStock + detail.Quantity) / estimatedMonthlyDemand, 1) : 1m;

            bool hasPriceAlert = priceVariance > 10.0m;
            bool hasQuantityAlert = monthsCoverage > 4.0m || detail.Quantity > (estimatedMonthlyDemand * 3);

            if (hasPriceAlert)
            {
                priceAlerts++;
                decimal overcostUnit = Math.Max(0, detail.UnitPrice - (lastPrice ?? detail.UnitPrice));
                totalPotentialOvercost += overcostUnit * detail.Quantity;
            }

            if (hasQuantityAlert)
            {
                quantityAlerts++;
            }

            totalVariancePct += priceVariance;

            string status = (hasPriceAlert && hasQuantityAlert) ? "Crítico" : (hasPriceAlert ? "Sobreprecio" : (hasQuantityAlert ? "Sobrecompra" : "Normal"));

            var itemDto = new FinancialItemAnalysisDto
            {
                ProductId = productId,
                ProductCode = product?.Code ?? "-",
                ProductName = product?.Name ?? "Producto sin nombre",
                CategoryName = product?.Category?.Name ?? "General",
                CurrentUnitPrice = detail.UnitPrice,
                LastPurchasedPrice = lastPrice,
                HistoricalAveragePrice = avgHistoricalPrice.HasValue ? Math.Round(avgHistoricalPrice.Value, 2) : null,
                PriceVariancePct = priceVariance,
                HasPriceAlert = hasPriceAlert,
                PriceInsight = hasPriceAlert
                    ? $"Precio unitario subió un +{priceVariance}% respecto a la última compra (${lastPrice:N2})."
                    : $"Precio unitario en rango esperado (${lastPrice:N2}).",
                RequestedQuantity = detail.Quantity,
                CurrentPhysicalStock = currentStock,
                ActiveStockThreshold = activeThreshold,
                MonthlyConsumptionAvg = estimatedMonthlyDemand,
                MonthsOfCoverage = monthsCoverage,
                HasQuantityAlert = hasQuantityAlert,
                QuantityInsight = hasQuantityAlert
                    ? $"Se solicitan {detail.Quantity} u. que cubren {monthsCoverage:0.#} meses de consumo (demanda est.: {estimatedMonthlyDemand} u./mes)."
                    : $"Cantidad equilibrada para {monthsCoverage:0.#} meses de operación.",
                ItemStatus = status
            };

            result.ItemsAnalysis.Add(itemDto);
        }

        result.AnomalousPriceCount = priceAlerts;
        result.AnomalousQuantityCount = quantityAlerts;
        result.PotentialOvercostAmount = Math.Round(totalPotentialOvercost, 2);
        result.AveragePriceVariancePct = result.ItemsAnalysis.Count > 0 ? Math.Round(totalVariancePct / result.ItemsAnalysis.Count, 1) : 0;

        await EnrichOrderWithGeminiLiveAsync(order, result);

        return result;
    }

    public async Task<GlobalInventoryAiAnalysisDto> AnalyzeGlobalInventoryAsync(int? warehouseId, DateTime startDate, DateTime endDate)
    {
        var warehouseQuery = _db.Warehouses.AsNoTracking();
        Warehouse? selectedWh = null;
        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            selectedWh = await warehouseQuery.FirstOrDefaultAsync(w => w.IdWarehouse == warehouseId.Value);
        }

        var lotsQuery = _db.Lots
            .Include(l => l.Product)
            .Where(l => l.IsActive && l.Quantity > 0);

        if (selectedWh != null)
        {
            lotsQuery = lotsQuery.Where(l => l.IdWarehouse == selectedWh.IdWarehouse);
        }

        var activeLots = await lotsQuery.AsNoTracking().ToListAsync();

        var today = DateTime.Today;
        int totalUnits = activeLots.Sum(l => l.Quantity);
        decimal totalValue = Math.Round(activeLots.Sum(l => l.Quantity * (l.Product?.SuggestedPrice ?? 0)), 2);
        int expiredLotsCount = activeLots.Count(l => l.ExpirationDate.Date < today);
        int expiringSoonCount = activeLots.Count(l => l.ExpirationDate.Date >= today && l.ExpirationDate.Date <= today.AddDays(30));

        var thresholdEntity = await _db.ThresholdInvetories.AsNoTracking().FirstOrDefaultAsync(t => t.IsActive);
        int activeThreshold = thresholdEntity?.Threshold ?? 5;

        var productQuantities = activeLots
            .GroupBy(l => l.IdProduct)
            .Select(g => new { ProductId = g.Key, Stock = g.Sum(x => x.Quantity) })
            .ToList();

        int lowStockCount = productQuantities.Count(p => p.Stock <= activeThreshold);

        var movementsCount = await _db.InventoryMovements
            .Where(m => m.MovementDate >= startDate && m.MovementDate <= endDate)
            .CountAsync();

        var dto = new GlobalInventoryAiAnalysisDto
        {
            TotalInventoryValue = totalValue,
            TotalUnitsCount = totalUnits,
            LowStockProductsCount = lowStockCount,
            ExpiredLotsCount = expiredLotsCount,
            ExpiringSoonLotsCount = expiringSoonCount,
            TotalMovementsCount = movementsCount,
            AnalyzedAt = DateTime.Now
        };

        await EnrichGlobalInventoryWithGeminiLiveAsync(selectedWh?.Name ?? "Consolidado Red de Clínicas", dto);

        return dto;
    }

    private async Task EnrichOrderWithGeminiLiveAsync(PurchaseOrder order, FinancialOrderAnalysisDto dto)
    {
        string? apiKey = _configuration["GeminiSettings:ApiKey"];
        string model = _configuration["GeminiSettings:Model"] ?? "gemini-3.5-flash";
        string baseUrl = _configuration["GeminiSettings:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("No se encontró la Gemini API Key en appsettings.json. Ingrésala para ejecutar el análisis cognitivo.");
        }

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Eres el Asistente Experto en Auditoría Financiera y Cadena de Suministro de 'MedVet Enterprise' (Clínica Veterinaria Integral).");
        promptBuilder.AppendLine("Analiza en tiempo real la siguiente orden de compra pendiente de aprobación financiera y emite un veredicto estructurado para el aprobador.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"DATOS DE LA ORDEN:");
        promptBuilder.AppendLine($"- Número: {order.OrderNumber}");
        promptBuilder.AppendLine($"- Proveedor: {order.Supplier?.CompanyName}");
        promptBuilder.AppendLine($"- Tipo: {order.PurchaseOrderType?.Name}");
        promptBuilder.AppendLine($"- Monto Total: ${order.TotalAmount:N2}");
        promptBuilder.AppendLine($"- Total Ítems: {dto.ItemsAnalysis.Count}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("DETALLE DE PRODUCTOS EVALUADOS:");

        foreach (var it in dto.ItemsAnalysis)
        {
            promptBuilder.AppendLine($"* {it.ProductName} (SKU: {it.ProductCode}):");
            promptBuilder.AppendLine($"  - Cantidad pedida: {it.RequestedQuantity} u. | Stock actual: {it.CurrentPhysicalStock} u. | Consumo mensual est.: {it.MonthlyConsumptionAvg} u.");
            promptBuilder.AppendLine($"  - Precio unitario: ${it.CurrentUnitPrice:N2} | Último precio: ${(it.LastPurchasedPrice.HasValue ? $"${it.LastPurchasedPrice:N2}" : "Sin compras previas")} | Variación: {it.PriceVariancePct:+#0.0;-#0.0;0}%");
            promptBuilder.AppendLine($"  - Meses de cobertura resultante: {it.MonthsOfCoverage:0.#} meses");
        }

        promptBuilder.AppendLine();
        promptBuilder.AppendLine("INSTRUCCIONES DE RESPUESTA:");
        promptBuilder.AppendLine("Responde ÚNICAMENTE en formato JSON plano con la siguiente estructura exacta:");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"riskLevel\": \"Bajo\" | \"Moderado\" | \"Alto\",");
        promptBuilder.AppendLine("  \"recommendation\": \"Aprobación Viable\" | \"Revisión Recomendada\" | \"Alerta Crítica de Gasto\",");
        promptBuilder.AppendLine("  \"executiveSummary\": \"Diagnóstico exhaustivo y profesional explicando la justificación financiera, impacto de sobreprecio y riesgo de meses de stock.\",");
        promptBuilder.AppendLine("  \"itemInsights\": [");
        promptBuilder.AppendLine("    {");
        promptBuilder.AppendLine("      \"productId\": 0,");
        promptBuilder.AppendLine("      \"priceInsight\": \"Evaluación detallada del precio pactado vs histórico\",");
        promptBuilder.AppendLine("      \"quantityInsight\": \"Evaluación detallada de la cantidad pedida vs meses de rotación\"");
        promptBuilder.AppendLine("    }");
        promptBuilder.AppendLine("  ]");
        promptBuilder.AppendLine("}");

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = promptBuilder.ToString() }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        string url = $"{baseUrl.TrimEnd('/')}/models/{model}:generateContent?key={apiKey}";
        var jsonPayload = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Google Gemini API devolvió error {response.StatusCode}: {errorContent}");
        }

        string responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var root = doc.RootElement;
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var candidateContent) &&
                candidateContent.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
            {
                string rawText = parts[0].GetProperty("text").GetString() ?? "";
                if (!string.IsNullOrWhiteSpace(rawText))
                {
                    var parsedResponse = JsonSerializer.Deserialize<GeminiAnalysisResponse>(rawText, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (parsedResponse != null)
                    {
                        dto.RiskLevel = !string.IsNullOrWhiteSpace(parsedResponse.RiskLevel) ? parsedResponse.RiskLevel.Trim() : "Moderado";
                        dto.Recommendation = !string.IsNullOrWhiteSpace(parsedResponse.Recommendation) ? parsedResponse.Recommendation.Trim() : "Revisión Recomendada";
                        dto.ExecutiveSummary = !string.IsNullOrWhiteSpace(parsedResponse.ExecutiveSummary) ? parsedResponse.ExecutiveSummary.Trim() : "Análisis generado por Gemini.";
                        dto.AiModelUsed = $"{model} (Google Gemini API - Generación en Vivo)";

                        if (parsedResponse.ItemInsights != null)
                        {
                            foreach (var insight in parsedResponse.ItemInsights)
                            {
                                var targetItem = dto.ItemsAnalysis.FirstOrDefault(i => i.ProductId == insight.ProductId);
                                if (targetItem != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(insight.PriceInsight))
                                        targetItem.PriceInsight = insight.PriceInsight.Trim();
                                    if (!string.IsNullOrWhiteSpace(insight.QuantityInsight))
                                        targetItem.QuantityInsight = insight.QuantityInsight.Trim();
                                }
                            }
                        }
                        return;
                    }
                }
            }
        }

        throw new InvalidOperationException("No se pudo interpretar la respuesta generada por Gemini API.");
    }

    private async Task EnrichGlobalInventoryWithGeminiLiveAsync(string warehouseName, GlobalInventoryAiAnalysisDto dto)
    {
        string? apiKey = _configuration["GeminiSettings:ApiKey"];
        string model = _configuration["GeminiSettings:Model"] ?? "gemini-3.5-flash";
        string baseUrl = _configuration["GeminiSettings:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("No se encontró la Gemini API Key en appsettings.json.");
        }

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Eres el Director de Inteligencia y Logística de MedVet Enterprise (Cadena de Clínicas Veterinarias).");
        promptBuilder.AppendLine($"Analiza los siguientes KPIs reales extraídos en vivo desde la base de datos SQL Server:");
        promptBuilder.AppendLine($"- Almacén / Alcance: {warehouseName}");
        promptBuilder.AppendLine($"- Unidades Físicas Totales en Stock: {dto.TotalUnitsCount} unidades");
        promptBuilder.AppendLine($"- Valorización del Inventario: ${dto.TotalInventoryValue:N2}");
        promptBuilder.AppendLine($"- Lotes Vencidos en Cuarentena: {dto.ExpiredLotsCount}");
        promptBuilder.AppendLine($"- Lotes Próximos a Vencer (30 días): {dto.ExpiringSoonLotsCount}");
        promptBuilder.AppendLine($"- Productos Bajo Umbral Mínimo: {dto.LowStockProductsCount}");
        promptBuilder.AppendLine($"- Movimientos Realizados en el Período: {dto.TotalMovementsCount}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("INSTRUCCIONES DE RESPUESTA:");
        promptBuilder.AppendLine("Genera un análisis ejecutivo real en formato JSON estructurado plano con:");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"executiveSummary\": \"Párrafo ejecutivo analizando la liquidez, capital inmovilizado y riesgo sanitario de caducidades.\",");
        promptBuilder.AppendLine("  \"findings\": [");
        promptBuilder.AppendLine("    {");
        promptBuilder.AppendLine("      \"type\": \"Sobre stock | Caducidad | Stock | Rotación | Margen\",");
        promptBuilder.AppendLine("      \"severity\": \"Alta\" | \"Media\" | \"Baja\",");
        promptBuilder.AppendLine("      \"message\": \"Descripción del hallazgo detectado\",");
        promptBuilder.AppendLine("      \"evidence\": \"Datos numéricos que respaldan el hallazgo\"");
        promptBuilder.AppendLine("    }");
        promptBuilder.AppendLine("  ],");
        promptBuilder.AppendLine("  \"recommendations\": [");
        promptBuilder.AppendLine("    {");
        promptBuilder.AppendLine("      \"action\": \"Acción operativa recomendada\",");
        promptBuilder.AppendLine("      \"target\": \"Área u objetivo\",");
        promptBuilder.AppendLine("      \"quantityLabel\": \"Cantidad sugerida o —\",");
        promptBuilder.AppendLine("      \"priority\": \"Alta\" | \"Media\" | \"Baja\"");
        promptBuilder.AppendLine("    }");
        promptBuilder.AppendLine("  ]");
        promptBuilder.AppendLine("}");

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = promptBuilder.ToString() } } }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        string url = $"{baseUrl.TrimEnd('/')}/models/{model}:generateContent?key={apiKey}";
        var jsonPayload = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Gemini API error {response.StatusCode}: {errorContent}");
        }

        string responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var root = doc.RootElement;
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var candidateContent) &&
                candidateContent.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
            {
                string rawText = parts[0].GetProperty("text").GetString() ?? "";
                if (!string.IsNullOrWhiteSpace(rawText))
                {
                    var parsed = JsonSerializer.Deserialize<GeminiGlobalResponse>(rawText, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (parsed != null)
                    {
                        dto.ExecutiveSummary = parsed.ExecutiveSummary ?? "Análisis completado.";
                        dto.AiModelUsed = $"{model} (Google Gemini API - Generación en Vivo)";

                        if (parsed.Findings != null)
                        {
                            dto.Findings = parsed.Findings.Select(f => new AiFindingDto
                            {
                                Type = f.Type ?? "General",
                                Severity = f.Severity ?? "Media",
                                Message = f.Message ?? "",
                                Evidence = f.Evidence ?? ""
                            }).ToList();
                        }

                        if (parsed.Recommendations != null)
                        {
                            dto.Recommendations = parsed.Recommendations.Select(r => new AiRecommendationDto
                            {
                                Action = r.Action ?? "",
                                Target = r.Target ?? "",
                                QuantityLabel = r.QuantityLabel ?? "—",
                                Priority = r.Priority ?? "Media"
                            }).ToList();
                        }

                        return;
                    }
                }
            }
        }

        throw new InvalidOperationException("No se pudo interpretar la respuesta generada por Gemini API.");
    }

    private class GeminiAnalysisResponse
    {
        public string? RiskLevel { get; set; }
        public string? Recommendation { get; set; }
        public string? ExecutiveSummary { get; set; }
        public List<GeminiItemInsight>? ItemInsights { get; set; }
    }

    private class GeminiItemInsight
    {
        public int ProductId { get; set; }
        public string? PriceInsight { get; set; }
        public string? QuantityInsight { get; set; }
    }

    private class GeminiGlobalResponse
    {
        public string? ExecutiveSummary { get; set; }
        public List<GeminiFindingItem>? Findings { get; set; }
        public List<GeminiRecItem>? Recommendations { get; set; }
    }

    private class GeminiFindingItem
    {
        public string? Type { get; set; }
        public string? Severity { get; set; }
        public string? Message { get; set; }
        public string? Evidence { get; set; }
    }

    private class GeminiRecItem
    {
        public string? Action { get; set; }
        public string? Target { get; set; }
        public string? QuantityLabel { get; set; }
        public string? Priority { get; set; }
    }
}
