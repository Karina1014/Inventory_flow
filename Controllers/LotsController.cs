using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Inventory_Flow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LotsController : ControllerBase
    {
        private readonly ILotRepository _lotRepository;
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public LotsController(
            ILotRepository lotRepository, 
            IUserContextService userContext,
            IMapper mapper, 
            IConfiguration configuration)
        {
            _lotRepository = lotRepository;
            _userContext = userContext;
            _mapper = mapper;
            _configuration = configuration;
        }

        // Helper para resolver el filtro de sucursal con política FAIL-SAFE DEFAULT
        private (bool IsAllowed, int? WarehouseId, IActionResult? ErrorResult) ResolveEffectiveWarehouse(int? requestedWarehouseId)
        {
            if (_userContext.IsAdmin)
            {
                return (true, requestedWarehouseId, null);
            }

            // Fail-Safe: Si no es Admin y no tiene sucursal en su claim -> Rechazar
            if (!_userContext.WarehouseId.HasValue)
            {
                return (false, null, StatusCode(StatusCodes.Status403Forbidden, "No tiene una sucursal asignada autorizada."));
            }

            return (true, _userContext.WarehouseId.Value, null);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetLots([FromQuery] int? idWarehouse)
        {
            var filter = ResolveEffectiveWarehouse(idWarehouse);
            if (!filter.IsAllowed) return filter.ErrorResult!;

            var lots = _lotRepository.GetLots(filter.WarehouseId);
            return Ok(MapLots(lots, LotExpiration.GetAlertDays(_configuration)));
        }

        [HttpGet("expiring")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetExpiringLots([FromQuery] int? days, [FromQuery] int? idWarehouse)
        {
            var filter = ResolveEffectiveWarehouse(idWarehouse);
            if (!filter.IsAllowed) return filter.ErrorResult!;

            var alertDays = days is > 0 ? days.Value : LotExpiration.GetAlertDays(_configuration);
            var lots = _lotRepository.GetExpiringLots(alertDays, filter.WarehouseId);
            return Ok(MapLots(lots, alertDays));
        }

        [HttpGet("expired")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetExpiredLots([FromQuery] int? idWarehouse)
        {
            var filter = ResolveEffectiveWarehouse(idWarehouse);
            if (!filter.IsAllowed) return filter.ErrorResult!;

            var lots = _lotRepository.GetExpiredLots(filter.WarehouseId);
            return Ok(MapLots(lots, LotExpiration.GetAlertDays(_configuration)));
        }

        [HttpGet("{IdLot:int}", Name = "GetLot")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetLot(int IdLot)
        {
            var lot = _lotRepository.GetLot(IdLot);
            if (lot == null)
            {
                return NotFound($"El lote con el id {IdLot} no existe");
            }

            // Control de acceso por sucursal
            if (!_userContext.IsAdmin && lot.IdWarehouse != _userContext.WarehouseId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "No tiene autorización para visualizar lotes de otra sucursal.");
            }

            var lotDto = _mapper.Map<LotDto>(lot);
            FillExpiration(lotDto, LotExpiration.GetAlertDays(_configuration));
            return Ok(lotDto);
        }

        private List<LotDto> MapLots(IEnumerable<Lot> lots, int alertDays)
        {
            var lotsDto = _mapper.Map<List<LotDto>>(lots);
            foreach (var dto in lotsDto)
            {
                FillExpiration(dto, alertDays);
            }
            return lotsDto;
        }

        private static void FillExpiration(LotDto dto, int alertDays)
        {
            dto.DaysToExpire = LotExpiration.DaysToExpire(dto.ExpirationDate);
            dto.IsExpired = dto.DaysToExpire < 0;
            dto.IsExpiringSoon = !dto.IsExpired && dto.DaysToExpire <= alertDays;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateLot([FromBody] CreateLotDto createLotDto)
        {
            if (createLotDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Operativos solo pueden registrar lotes en su sucursal
            if (!_userContext.IsAdmin)
            {
                if (!_userContext.WarehouseId.HasValue || createLotDto.IdWarehouse != _userContext.WarehouseId.Value)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "Solo puede crear lotes en su sucursal asignada.");
                }
            }

            var result = await _lotRepository.CreateLot(createLotDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el lote");
            }
            return CreatedAtRoute("GetLot", new { IdLot = result.IdLot }, result);
        }

        [HttpPut("{IdLot:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLot(int IdLot, [FromBody] UpdateLotDto updateLotDto)
        {
            if (updateLotDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = _lotRepository.GetLot(IdLot);
            if (existing == null)
            {
                return NotFound($"El lote con el id {IdLot} no existe");
            }

            if (!_userContext.IsAdmin && existing.IdWarehouse != _userContext.WarehouseId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "No tiene autorización para modificar lotes de otra sucursal.");
            }

            await _lotRepository.UpdateLot(IdLot, updateLotDto);
            return NoContent();
        }

        [HttpDelete("{IdLot:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteLot(int IdLot)
        {
            var existing = _lotRepository.GetLot(IdLot);
            if (existing == null)
            {
                return NotFound($"El lote con el id {IdLot} no existe");
            }

            if (!_userContext.IsAdmin && existing.IdWarehouse != _userContext.WarehouseId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "No tiene autorización para eliminar lotes de otra sucursal.");
            }

            if (!_lotRepository.DeleteLot(IdLot))
            {
                return NotFound($"El lote con el id {IdLot} no existe");
            }
            return NoContent();
        }
    }
}
