using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        // Estado 2: PENDIENTE_APROBACION_FINANCIERA
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

            // 1. Stock físico actual en la bodega destino
            int currentStock = await _db.Lots
                .Where(l => l.IdProduct == productId && l.IdWarehouse == order.IdDestinationWarehouse && l.IsActive && l.Quantity > 0)
                .SumAsync(l => (int?)l.Quantity) ?? 0;

            // 2. Historial de compras anteriores del mismo producto
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

            // 3. Estimación de consumo mensual basado en rotación de movimientos
            var sixtyDaysAgo = DateTime.Today.AddDays(-60);
            var movementsOutCount = await _db.InventoryMovementDetails
                .Include(imd => imd.InventoryMovement)
                .Where(imd => imd.IdProduct == productId && imd.InventoryMovement!.MovementDate >= sixtyDaysAgo && imd.InventoryMovement.IdOriginWarehouse == order.IdDestinationWarehouse)
                .SumAsync(imd => (int?)imd.Quantity) ?? 0;

            int estimatedMonthlyDemand = Math.Max((int)Math.Round(movementsOutCount / 2.0), Math.Max(activeThreshold, 8));
            decimal monthsCoverage = estimatedMonthlyDemand > 0 ? Math.Round((decimal)(currentStock + detail.Quantity) / estimatedMonthlyDemand, 1) : 1m;

            // Reglas de alerta preliminar
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

        // Intentar enriquecer con Google Gemini API
        bool geminiSuccess = await TryEnrichWithGeminiAsync(order, result);

        if (!geminiSuccess)
        {
            ApplyLocalHeuristicVerdict(result);
        }

        return result;
    }

    private async Task<bool> TryEnrichWithGeminiAsync(PurchaseOrder order, FinancialOrderAnalysisDto dto)
    {
        string? apiKey = _configuration["GeminiSettings:ApiKey"];
        string model = _configuration["GeminiSettings:Model"] ?? "gemini-2.0-flash";
        string baseUrl = _configuration["GeminiSettings:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API Key no está configurada. Usando motor de diagnóstico local.");
            return false;
        }

        try
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Eres el Asistente Experto en Auditoría Financiera y Cadena de Suministro de 'MedVet Enterprise' (Clínica Veterinaria Integral).");
            promptBuilder.AppendLine("Analiza la siguiente orden de compra pendiente de aprobación financiera y emite un veredicto estructurado para el aprobador.");
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
            promptBuilder.AppendLine("Responde ÚNICAMENTE en formato JSON plano con la siguiente estructura exacta sin texto adicional:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"riskLevel\": \"Bajo\" | \"Moderado\" | \"Alto\",");
            promptBuilder.AppendLine("  \"recommendation\": \"Aprobación Viable\" | \"Revisión Recomendada\" | \"Alerta Crítica de Gasto\",");
            promptBuilder.AppendLine("  \"executiveSummary\": \"Resumen ejecutivo claro de 2 a 3 frases explicando si conviene aprobar, negociar precio o reducir cantidad.\",");
            promptBuilder.AppendLine("  \"itemInsights\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"productId\": 0,");
            promptBuilder.AppendLine("      \"priceInsight\": \"Explicación corta sobre el precio de este producto\",");
            promptBuilder.AppendLine("      \"quantityInsight\": \"Explicación corta sobre la cantidad pedida\"");
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
            client.Timeout = TimeSpan.FromSeconds(25);

            string url = $"{baseUrl.TrimEnd('/')}/models/{model}:generateContent?key={apiKey}";
            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Gemini API respondió con código {StatusCode}: {Error}", response.StatusCode, errorContent);
                return false;
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
                            if (!string.IsNullOrWhiteSpace(parsedResponse.RiskLevel))
                            {
                                dto.RiskLevel = parsedResponse.RiskLevel.Trim();
                            }
                            if (!string.IsNullOrWhiteSpace(parsedResponse.Recommendation))
                            {
                                dto.Recommendation = parsedResponse.Recommendation.Trim();
                            }
                            if (!string.IsNullOrWhiteSpace(parsedResponse.ExecutiveSummary))
                            {
                                dto.ExecutiveSummary = parsedResponse.ExecutiveSummary.Trim();
                            }

                            dto.AiModelUsed = $"{model} (Google Gemini API)";

                            if (parsedResponse.ItemInsights != null && parsedResponse.ItemInsights.Count > 0)
                            {
                                foreach (var insight in parsedResponse.ItemInsights)
                                {
                                    var targetItem = dto.ItemsAnalysis.FirstOrDefault(i => i.ProductId == insight.ProductId);
                                    if (targetItem != null)
                                    {
                                        if (!string.IsNullOrWhiteSpace(insight.PriceInsight))
                                        {
                                            targetItem.PriceInsight = insight.PriceInsight.Trim();
                                        }
                                        if (!string.IsNullOrWhiteSpace(insight.QuantityInsight))
                                        {
                                            targetItem.QuantityInsight = insight.QuantityInsight.Trim();
                                        }
                                    }
                                }
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comunicando con Gemini API para análisis de orden {OrderNumber}", order.OrderNumber);
            return false;
        }
    }

    private void ApplyLocalHeuristicVerdict(FinancialOrderAnalysisDto dto)
    {
        dto.AiModelUsed = "Motor Heurístico Local (Respaldo)";

        if (dto.AnomalousPriceCount >= 2 || dto.PotentialOvercostAmount > 100 || dto.ItemsAnalysis.Any(i => i.ItemStatus == "Crítico"))
        {
            dto.RiskLevel = "Alto";
            dto.Recommendation = "Alerta Crítica de Gasto";
            dto.ExecutiveSummary = $"Se detectaron incrementos de precio significativos ({dto.AveragePriceVariancePct:+#0.0}% prom.) con sobrecosto estimado de ${dto.PotentialOvercostAmount:N2}. Se recomienda renegociar con el proveedor antes de aprobar.";
        }
        else if (dto.AnomalousPriceCount > 0 || dto.AnomalousQuantityCount > 0)
        {
            dto.RiskLevel = "Moderado";
            dto.Recommendation = "Revisión Recomendada";
            dto.ExecutiveSummary = $"La orden presenta desviaciones menores en {dto.AnomalousPriceCount} precios y {dto.AnomalousQuantityCount} cantidades respecto al histórico de consumo. Aprobación viable con supervisión.";
        }
        else
        {
            dto.RiskLevel = "Bajo";
            dto.Recommendation = "Aprobación Viable";
            dto.ExecutiveSummary = "Todos los precios pactados se alinean con las compras anteriores del catálogo y las cantidades solicitadas responden a la demanda mensual estimada sin riesgo de sobre-stock.";
        }
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
}
