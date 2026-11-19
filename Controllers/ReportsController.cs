using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Inventory_Flow.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly ILowStockReportService _reportService;
    private readonly IMatrixStockReportService _matrixReportService;
    private readonly AplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public ReportsController(
        ILowStockReportService reportService,
        IMatrixStockReportService matrixReportService,
        AplicationDbContext db,
        IConfiguration configuration)
    {
        _reportService = reportService;
        _matrixReportService = matrixReportService;
        _db = db;
        _configuration = configuration;
    }

    private bool IsAuthorized(string? apiKey)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return true;
        }

        var configuredKey = _configuration["ReportSettings:ApiKey"] ?? "MedVet_Report_ApiKey_2026_Secure";

        // Revisar por cabecera HTTP
        if (Request.Headers.TryGetValue("X-Api-Key", out var headerKey) && !string.IsNullOrWhiteSpace(headerKey))
        {
            if (string.Equals(headerKey.ToString().Trim(), configuredKey, StringComparison.Ordinal))
            {
                return true;
            }
        }

        // Revisar por Query String
        if (!string.IsNullOrWhiteSpace(apiKey) && string.Equals(apiKey.Trim(), configuredKey, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    [HttpGet("warehouses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveWarehouses([FromQuery] string? apiKey = null)
    {
        if (!IsAuthorized(apiKey))
        {
            return Unauthorized(new { message = "Acceso no autorizado. Proporcione la API Key en el parámetro 'apiKey' o en la cabecera 'X-Api-Key'." });
        }

        var warehouses = await _db.Warehouses
            .AsNoTracking()
            .Where(w => w.IsActive)
            .Select(w => new
            {
                w.IdWarehouse,
                w.Name,
                w.Address
            })
            .ToListAsync();

        return Ok(warehouses);
    }

    [HttpGet("low-stock/warehouses/{idWarehouse:int}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLowStockPdf(int idWarehouse, [FromQuery] string? apiKey = null)
    {
        if (!IsAuthorized(apiKey))
        {
            return Unauthorized("Acceso no autorizado. Proporcione la API Key en el parámetro 'apiKey' o en la cabecera 'X-Api-Key'.");
        }

        try
        {
            var pdfBytes = await _reportService.GenerateLowStockPdfByWarehouseAsync(idWarehouse);
            var fileName = $"BajoStock_Sucursal_{idWarehouse}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");
            return File(pdfBytes, "application/pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            var fullError = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return BadRequest($"Error generando el reporte PDF de bajo stock: {fullError}");
        }
    }

    [HttpGet("low-stock/warehouses/{idWarehouse:int}/summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLowStockSummary(int idWarehouse, [FromQuery] string? apiKey = null)
    {
        if (!IsAuthorized(apiKey))
        {
            return Unauthorized(new { message = "Acceso no autorizado. Proporcione la API Key en el parámetro 'apiKey' o en la cabecera 'X-Api-Key'." });
        }

        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var summary = await _reportService.GetLowStockSummaryByWarehouseAsync(idWarehouse, baseUrl);

            // Si se usó apiKey, anexarla a la URL del PDF para que n8n pueda descargarla directamente
            var configuredKey = _configuration["ReportSettings:ApiKey"] ?? "MedVet_Report_ApiKey_2026_Secure";
            var effectiveKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : configuredKey;
            summary.PdfDownloadUrl += $"?apiKey={effectiveKey}";

            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Genera y descarga el documento PDF de existencias y caducidades de la Bodega Matriz Carapungo.
    /// Utilizado por el Bodeguero Principal y enviado automáticamente por n8n + Evolution API.
    /// </summary>
    [HttpGet("matrix-inventory/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMatrixStockPdf([FromQuery] string? apiKey = null)
    {
        if (!IsAuthorized(apiKey))
        {
            return Unauthorized("Acceso no autorizado. Proporcione la API Key en el parámetro 'apiKey' o en la cabecera 'X-Api-Key'.");
        }

        try
        {
            var pdfBytes = await _matrixReportService.GenerateMatrixStockPdfAsync();
            var fileName = $"Inventario_Matriz_Carapungo_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");
            return File(pdfBytes, "application/pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            var fullError = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return BadRequest($"Error generando el reporte PDF de la Matriz: {fullError}");
        }
    }

    /// <summary>
    /// Genera y descarga el documento PDF de existencias y lotes de CUALQUIER sucursal elegida por su ID.
    /// </summary>
    [HttpGet("inventory/warehouses/{idWarehouse:int}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWarehouseStockPdf(int idWarehouse, [FromQuery] string? apiKey = null)
    {
        if (!IsAuthorized(apiKey))
        {
            return Unauthorized("Acceso no autorizado. Proporcione la API Key en el parámetro 'apiKey' o en la cabecera 'X-Api-Key'.");
        }

        try
        {
            var pdfBytes = await _matrixReportService.GenerateMatrixStockPdfAsync(idWarehouse);
            var fileName = $"Inventario_Sucursal_{idWarehouse}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");
            return File(pdfBytes, "application/pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            var fullError = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return BadRequest($"Error generando el reporte PDF de la sucursal: {fullError}");
        }
    }

    /// <summary>
    /// Retorna un resumen en formato JSON con métricas de la Matriz Carapungo y los lotes más próximos a vencer.
    /// Especialmente diseñado para n8n antes de disparar el webhook de Evolution API.
    /// </summary>
    [HttpGet("matrix-inventory/summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMatrixStockSummary([FromQuery] string? apiKey = null)
    {
        if (!IsAuthorized(apiKey))
        {
            return Unauthorized(new { message = "Acceso no autorizado. Proporcione la API Key en el parámetro 'apiKey' o en la cabecera 'X-Api-Key'." });
        }

        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var summary = await _matrixReportService.GetMatrixStockSummaryAsync(null, baseUrl);

            var configuredKey = _configuration["ReportSettings:ApiKey"] ?? "MedVet_Report_ApiKey_2026_Secure";
            var effectiveKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : configuredKey;
            summary.PdfDownloadUrl += $"?apiKey={effectiveKey}";

            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
