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
    public class InternalRequisitionsController : ControllerBase
    {
        private readonly IInternalRequisitionRepository _requisitionRepo;
        private readonly IRequisitionCatalogRepository _catalogRepo;
        private readonly IMapper _mapper;

        public InternalRequisitionsController(
            IInternalRequisitionRepository requisitionRepo,
            IRequisitionCatalogRepository catalogRepo,
            IMapper mapper)
        {
            _requisitionRepo = requisitionRepo;
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
            return Ok(_mapper.Map<List<RequisitionStatusDto>>(statuses));
        }

        [HttpGet("types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTypes([FromQuery] bool activeOnly = true)
        {
            var types = await _catalogRepo.GetTypesAsync(activeOnly);
            return Ok(_mapper.Map<List<RequisitionTypeDto>>(types));
        }

        // ---------- Requisiciones ----------

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? requestingWarehouseId = null,
            [FromQuery] string? statusCode = null,
            [FromQuery] int? requisitionTypeId = null,
            [FromQuery] string? search = null,
            [FromQuery] bool prioritizeByType = false)
        {
            var paged = await _requisitionRepo.GetRequisitionsPagedAsync(
                pageNumber, pageSize, requestingWarehouseId, statusCode, requisitionTypeId, search, prioritizeByType);

            return Ok(new PagedResultDto<InternalRequisitionDto>
            {
                Items = _mapper.Map<List<InternalRequisitionDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            });
        }

        [HttpGet("{id:int}", Name = "GetInternalRequisition")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var req = await _requisitionRepo.GetRequisitionByIdAsync(id);
            if (req == null)
            {
                return NotFound($"La requisición con id {id} no existe.");
            }

            return Ok(_mapper.Map<InternalRequisitionDto>(req));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateInternalRequisitionDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized("No se pudo identificar al usuario solicitante.");
            }

            try
            {
                var created = await _requisitionRepo.CreateRequisitionAsync(dto, userId);
                var full = await _requisitionRepo.GetRequisitionByIdAsync(created.IdInternalRequisition);
                return CreatedAtRoute("GetInternalRequisition",
                    new { id = created.IdInternalRequisition },
                    _mapper.Map<InternalRequisitionDto>(full));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateInternalRequisitionStatusDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized("No se pudo identificar al usuario revisor.");
            }

            try
            {
                var success = await _requisitionRepo.UpdateStatusAsync(id, dto.NewStatusCode, userId, dto.ReviewNote);
                return success ? NoContent() : NotFound($"La requisición con id {id} no existe.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
