using AutoMapper;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryMovementsController : ControllerBase
    {
        private readonly IInventoryMovementRepository _inventoryMovementRepository;
        private readonly IMapper _mapper;

        public InventoryMovementsController(IInventoryMovementRepository inventoryMovementRepository, IMapper mapper)
        {
            _inventoryMovementRepository = inventoryMovementRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetInventoryMovements()
        {
            var movements = _inventoryMovementRepository.GetInventoryMovements();
            var movementsDto = _mapper.Map<List<InventoryMovementDto>>(movements);
            return Ok(movementsDto);
        }

        [HttpGet("{IdInventoryMovement:int}", Name = "GetInventoryMovement")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetInventoryMovement(int IdInventoryMovement)
        {
            var movement = _inventoryMovementRepository.GetInventoryMovement(IdInventoryMovement);
            if (movement == null)
            {
                return NotFound($"El movimiento de inventario con el id {IdInventoryMovement} no existe");
            }
            var movementDto = _mapper.Map<InventoryMovementDto>(movement);
            return Ok(movementDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateInventoryMovement([FromBody] CreateInventoryMovementDto createInventoryMovementDto)
        {
            if (createInventoryMovementDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!_inventoryMovementRepository.IsUniqueCode(createInventoryMovementDto.Code))
            {
                return BadRequest("Ya existe un movimiento con ese código.");
            }
            var result = await _inventoryMovementRepository.CreateInventoryMovement(createInventoryMovementDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el movimiento de inventario");
            }
            return CreatedAtRoute("GetInventoryMovement", new { IdInventoryMovement = result.IdInventoryMovement }, result);
        }

        [HttpPut("{IdInventoryMovement:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateInventoryMovement(int IdInventoryMovement, [FromBody] UpdateInventoryMovementDto updateInventoryMovementDto)
        {
            if (updateInventoryMovementDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_inventoryMovementRepository.GetInventoryMovement(IdInventoryMovement) == null)
            {
                return NotFound($"El movimiento de inventario con el id {IdInventoryMovement} no existe");
            }
            await _inventoryMovementRepository.UpdateInventoryMovement(IdInventoryMovement, updateInventoryMovementDto);
            return NoContent();
        }

        [HttpDelete("{IdInventoryMovement:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteInventoryMovement(int IdInventoryMovement)
        {
            if (!_inventoryMovementRepository.DeleteInventoryMovement(IdInventoryMovement))
            {
                return NotFound($"El movimiento de inventario con el id {IdInventoryMovement} no existe");
            }
            return NoContent();
        }
    }
}
