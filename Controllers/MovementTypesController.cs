using AutoMapper;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovementTypesController : ControllerBase
    {
        private readonly IMovementTypeRepository _inventoryMovementRepository;
        private readonly IMapper _mapper;

        public MovementTypesController(IMovementTypeRepository inventoryMovementRepository, IMapper mapper)
        {
            _inventoryMovementRepository = inventoryMovementRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetMovementTypes()
        {
            var movements = _inventoryMovementRepository.GetMovementTypes();
            var movementsDto = _mapper.Map<List<MovementTypeDto>>(movements);
            return Ok(movementsDto);
        }

        [HttpGet("{IdMovementType:int}", Name = "GetMovementType")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetMovementType(int IdMovementType)
        {
            var movement = _inventoryMovementRepository.GetMovementType(IdMovementType);
            if (movement == null)
            {
                return NotFound($"El movimiento de inventario con el id {IdMovementType} no existe");
            }
            var movementDto = _mapper.Map<MovementTypeDto>(movement);
            return Ok(movementDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMovementType([FromBody] CreateMovementTypeDto createMovementTypeDto)
        {
            if (createMovementTypeDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!_inventoryMovementRepository.IsUniqueCode(createMovementTypeDto.Code))
            {
                return BadRequest("Ya existe un movimiento con ese código.");
            }
            var result = await _inventoryMovementRepository.CreateMovementType(createMovementTypeDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el movimiento de inventario");
            }
            return CreatedAtRoute("GetMovementType", new { IdMovementType = result.IdMovementType }, result);
        }

        [HttpPut("{IdMovementType:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMovementType(int IdMovementType, [FromBody] UpdateMovementTypeDto updateMovementTypeDto)
        {
            if (updateMovementTypeDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_inventoryMovementRepository.GetMovementType(IdMovementType) == null)
            {
                return NotFound($"El movimiento de inventario con el id {IdMovementType} no existe");
            }
            await _inventoryMovementRepository.UpdateMovementType(IdMovementType, updateMovementTypeDto);
            return NoContent();
        }

        [HttpDelete("{IdMovementType:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteMovementType(int IdMovementType)
        {
            if (!_inventoryMovementRepository.DeleteMovementType(IdMovementType))
            {
                return NotFound($"El movimiento de inventario con el id {IdMovementType} no existe");
            }
            return NoContent();
        }
    }
}
