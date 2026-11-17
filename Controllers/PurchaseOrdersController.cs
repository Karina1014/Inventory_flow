using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderRepository _orderRepo;
        private readonly IPurchaseOrderCatalogRepository _catalogRepo;
        private readonly IMapper _mapper;

        public PurchaseOrdersController(
            IPurchaseOrderRepository orderRepo,
            IPurchaseOrderCatalogRepository catalogRepo,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _catalogRepo = catalogRepo;
            _mapper = mapper;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : 0;
        }

        // ---------- Catálogos ----------

        [HttpGet("statuses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatuses()
        {
            var statuses = await _catalogRepo.GetStatusesAsync();
            return Ok(_mapper.Map<List<PurchaseOrderStatusDto>>(statuses));
        }

        [HttpGet("types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTypes([FromQuery] bool activeOnly = true)
        {
            var types = await _catalogRepo.GetTypesAsync(activeOnly);
            return Ok(_mapper.Map<List<PurchaseOrderTypeDto>>(types));
        }

        // ---------- Órdenes de Compra ----------

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? supplierId = null,
            [FromQuery] int? statusId = null,
            [FromQuery] int? orderTypeId = null,
            [FromQuery] string? search = null,
            [FromQuery] bool pendingFinancialOnly = false)
        {
            var paged = await _orderRepo.GetOrdersPagedAsync(
                pageNumber, pageSize, supplierId, statusId, orderTypeId, search, pendingFinancialOnly);

            return Ok(new PagedResultDto<PurchaseOrderDto>
            {
                Items = _mapper.Map<List<PurchaseOrderDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            });
        }

        [HttpGet("{id:int}", Name = "GetPurchaseOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound($"La orden de compra con id {id} no existe.");
            }

            return Ok(_mapper.Map<PurchaseOrderDto>(order));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized("No se pudo identificar al usuario que genera la orden.");
            }

            try
            {
                var created = await _orderRepo.CreateOrderAsync(dto, userId);
                var full = await _orderRepo.GetOrderByIdAsync(created.IdPurchaseOrder);
                return CreatedAtRoute("GetPurchaseOrder",
                    new { id = created.IdPurchaseOrder },
                    _mapper.Map<PurchaseOrderDto>(full));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}/financial-approval")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinancialApproval(int id, [FromBody] FinancialApprovalDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Datos de aprobación inválidos.");
            }

            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized("No se pudo autenticar al usuario financiero.");
            }

            try
            {
                var success = await _orderRepo.ProcessFinancialApprovalAsync(id, dto.Approved, userId, dto.Note);
                return success ? NoContent() : NotFound($"La orden con id {id} no existe.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
