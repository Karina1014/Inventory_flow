using System;
using System.Threading.Tasks;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardRepository _dashboardRepo;

    public DashboardController(IDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardMetricsDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardMetricsDto>> GetMetrics()
    {
        try
        {
            var metrics = await _dashboardRepo.GetDashboardMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new 
            { 
                message = "Error al calcular las métricas del dashboard.", 
                detail = ex.Message 
            });
        }
    }
}

