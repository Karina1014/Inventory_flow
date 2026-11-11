using AutoMapper;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LotsController : ControllerBase
    {
        private readonly ILotRepository _lotRepository;
        private readonly IMapper _mapper;

        public LotsController(ILotRepository lotRepository, IMapper mapper)
        {
            _lotRepository = lotRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetLots()
        {
            var lots = _lotRepository.GetLots();
            var lotsDto = _mapper.Map<List<LotDto>>(lots);
            return Ok(lotsDto);
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
            var lotDto = _mapper.Map<LotDto>(lot);
            return Ok(lotDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateLot([FromBody] CreateLotDto createLotDto)
        {
            if (createLotDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLot(int IdLot, [FromBody] UpdateLotDto updateLotDto)
        {
            if (updateLotDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_lotRepository.GetLot(IdLot) == null)
            {
                return NotFound($"El lote con el id {IdLot} no existe");
            }
            await _lotRepository.UpdateLot(IdLot, updateLotDto);
            return NoContent();
        }

        [HttpDelete("{IdLot:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteLot(int IdLot)
        {
            if (!_lotRepository.DeleteLot(IdLot))
            {
                return NotFound($"El lote con el id {IdLot} no existe");
            }
            return NoContent();
        }
    }
}
