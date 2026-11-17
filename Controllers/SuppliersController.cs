using System.Collections.Generic;
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
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IMapper _mapper;

        public SuppliersController(ISupplierRepository supplierRepository, IMapper mapper)
        {
            _supplierRepository = supplierRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetSuppliers([FromQuery] bool? activeOnly = null)
        {
            var suppliers = _supplierRepository.GetSuppliers();
            if (activeOnly.HasValue && activeOnly.Value)
            {
                suppliers = suppliers.Where(s => s.IsActive).ToList();
            }
            var suppliersDto = _mapper.Map<List<SupplierDto>>(suppliers);
            return Ok(suppliersDto);
        }

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSuppliersPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? activeOnly = null)
        {
            var result = await _supplierRepository.GetSuppliersPagedAsync(pageNumber, pageSize, search, activeOnly);
            return Ok(result);
        }

        [HttpGet("{IdSupplier:int}", Name = "GetSupplier")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetSupplier(int IdSupplier)
        {
            var supplier = _supplierRepository.GetSupplier(IdSupplier);
            if (supplier == null)
            {
                return NotFound($"El proveedor con Id {IdSupplier} no existe.");
            }
            var supplierDto = _mapper.Map<SupplierDto>(supplier);
            return Ok(supplierDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto createSupplierDto)
        {
            if (createSupplierDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_supplierRepository.IsUniqueRuc(createSupplierDto.Ruc))
            {
                return BadRequest("Ya existe un proveedor registrado con ese número de RUC.");
            }

            var result = await _supplierRepository.CreateSupplier(createSupplierDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al registrar el proveedor.");
            }

            return CreatedAtRoute("GetSupplier", new { IdSupplier = result.IdSupplier }, result);
        }

        [HttpPut("{IdSupplier:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSupplier(int IdSupplier, [FromBody] UpdateSupplierDto updateSupplierDto)
        {
            if (updateSupplierDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_supplierRepository.GetSupplier(IdSupplier) == null)
            {
                return NotFound($"El proveedor con Id {IdSupplier} no existe.");
            }

            if (!_supplierRepository.IsUniqueRuc(updateSupplierDto.Ruc, IdSupplier))
            {
                return BadRequest("Ya existe otro proveedor registrado con ese número de RUC.");
            }

            await _supplierRepository.UpdateSupplier(IdSupplier, updateSupplierDto);
            return NoContent();
        }

        [HttpDelete("{IdSupplier:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteSupplier(int IdSupplier)
        {
            if (!_supplierRepository.DeleteSupplier(IdSupplier))
            {
                return NotFound($"El proveedor con Id {IdSupplier} no existe.");
            }
            return NoContent();
        }
    }
}
