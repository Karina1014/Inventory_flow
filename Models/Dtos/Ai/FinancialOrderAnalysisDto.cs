using System;
using System.Collections.Generic;

namespace Inventory_Flow.Models.Dtos.Ai;
public class FinancialOrderAnalysisDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    // Evaluación Global
    public string RiskLevel { get; set; } = "Bajo"; // "Bajo", "Moderado", "Alto"
    public string RiskBadgeClass => RiskLevel switch
    {
        "Alto" => "badge-danger",
        "Moderado" => "badge-warning",
        _ => "badge-success"
    };

    public string Recommendation { get; set; } = "Aprobación Viable"; // "Aprobación Viable", "Revisión Recomendada", "Alerta Crítica de Gasto"
    public string RecommendationBadgeClass => Recommendation switch
    {
        "Alerta Crítica de Gasto" => "badge-danger",
        "Revisión Recomendada" => "badge-warning",
        _ => "badge-success"
    };

    public string ExecutiveSummary { get; set; } = string.Empty;
    public string AiModelUsed { get; set; } = "Gemini 2.0 Flash";
    public DateTime AnalyzedAt { get; set; } = DateTime.Now;

    // Métricas Resumen
    public decimal PotentialOvercostAmount { get; set; }
    public decimal AveragePriceVariancePct { get; set; }
    public int AnomalousPriceCount { get; set; }
    public int AnomalousQuantityCount { get; set; }
    public int TotalItemsCount { get; set; }

    // Detalle Ítem por Ítem
    public List<FinancialItemAnalysisDto> ItemsAnalysis { get; set; } = new();
}

public class FinancialItemAnalysisDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    // Precios
    public decimal CurrentUnitPrice { get; set; }
    public decimal? LastPurchasedPrice { get; set; }
    public decimal? HistoricalAveragePrice { get; set; }
    public decimal PriceVariancePct { get; set; }
    public bool HasPriceAlert { get; set; }
    public string PriceInsight { get; set; } = string.Empty;

    // Cantidades y Demanda
    public int RequestedQuantity { get; set; }
    public int CurrentPhysicalStock { get; set; }
    public int ActiveStockThreshold { get; set; }
    public int MonthlyConsumptionAvg { get; set; }
    public decimal MonthsOfCoverage { get; set; }
    public bool HasQuantityAlert { get; set; }
    public string QuantityInsight { get; set; } = string.Empty;

    // Estado del ítem
    public string ItemStatus { get; set; } = "Normal"; // "Normal", "Sobreprecio", "Sobrecompra", "Crítico"
    public string ItemBadgeClass => ItemStatus switch
    {
        "Crítico" or "Sobreprecio" => "badge-danger",
        "Sobrecompra" => "badge-warning",
        _ => "badge-success"
    };
}

public class GlobalInventoryAiAnalysisDto
{
    public decimal TotalInventoryValue { get; set; }
    public int TotalUnitsCount { get; set; }
    public int LowStockProductsCount { get; set; }
    public int ExpiredLotsCount { get; set; }
    public int ExpiringSoonLotsCount { get; set; }
    public int TotalMovementsCount { get; set; }
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string AiModelUsed { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; } = DateTime.Now;
    public List<AiFindingDto> Findings { get; set; } = new();
    public List<AiRecommendationDto> Recommendations { get; set; } = new();
}

public class AiFindingDto
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = "Media";
    public string Message { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    public string SeverityClass => Severity switch
    {
        "Alta" => "sev-alta",
        "Media" => "sev-media",
        _ => "sev-baja"
    };
}

public class AiRecommendationDto
{
    public string Action { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string QuantityLabel { get; set; } = string.Empty;
    public string Priority { get; set; } = "Media";
    public string PriorityClass => Priority switch
    {
        "Alta" => "sev-alta",
        "Media" => "sev-media",
        _ => "sev-baja"
    };
}
