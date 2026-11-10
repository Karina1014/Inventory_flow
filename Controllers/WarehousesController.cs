using AutoMapper;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IMapper _mapper;

        public WarehousesController(IWarehouseRepository warehouseRepository, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetWarehouses()
        {
            var warehouses = _warehouseRepository.GetWarehouses();
            var warehousesDto = _mapper.Map<List<WarehouseDto>>(warehouses);
            return Ok(warehousesDto);
        }

        [HttpGet("{IdWarehouse:int}", Name = "GetWarehouse")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetWarehouse(int IdWarehouse)
        {
            var warehouse = _warehouseRepository.GetWarehouse(IdWarehouse);
            if (warehouse == null)
            {
                return NotFound($"El almacén con el id {IdWarehouse} no existe");
            }
            var warehouseDto = _mapper.Map<WarehouseDto>(warehouse);
            return Ok(warehouseDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto createWarehouseDto)
        {
            if (createWarehouseDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!_warehouseRepository.IsUniqueName(createWarehouseDto.Name))
            {
                return BadRequest("Ya existe un almacén con ese nombre.");
            }
            var result = await _warehouseRepository.CreateWarehouse(createWarehouseDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el almacén");
            }
            return CreatedAtRoute("GetWarehouse", new { IdWarehouse = result.IdWarehouse }, result);
        }

        [HttpPut("{IdWarehouse:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWarehouse(int IdWarehouse, [FromBody] UpdateWarehouseDto updateWarehouseDto)
        {
            if (updateWarehouseDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_warehouseRepository.GetWarehouse(IdWarehouse) == null)
            {
                return NotFound($"El almacén con el id {IdWarehouse} no existe");
            }
            await _warehouseRepository.UpdateWarehouse(IdWarehouse, updateWarehouseDto);
            return NoContent();
        }

        [HttpDelete("{IdWarehouse:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteWarehouse(int IdWarehouse)
        {
            if (!_warehouseRepository.DeleteWarehouse(IdWarehouse))
            {
                return NotFound($"El almacén con el id {IdWarehouse} no existe");
            }
            return NoContent();
        }
    }
}

