using AutoMapper;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RolesController(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetRoles()
        {
            var roles = _roleRepository.GetRoles();
            var rolesDto = _mapper.Map<List<RoleDto>>(roles);
            return Ok(rolesDto);
        }

        [HttpGet("{IdRole:int}", Name = "GetRole")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetRole(int IdRole)
        {
            var role = _roleRepository.GetRole(IdRole);
            if (role == null)
            {
                return NotFound($"El rol con el id {IdRole} no existe");
            }
            var roleDto = _mapper.Map<RoleDto>(role);
            return Ok(roleDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (createRoleDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!_roleRepository.IsUniqueName(createRoleDto.Name))
            {
                return BadRequest("Ya existe un rol con ese nombre.");
            }
            var result = await _roleRepository.CreateRole(createRoleDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el rol");
            }
            return CreatedAtRoute("GetRole", new { IdRole = result.IdRole }, result);
        }

        [HttpPut("{IdRole:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRole(int IdRole, [FromBody] CreateRoleDto updateRoleDto)
        {
            if (updateRoleDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_roleRepository.GetRole(IdRole) == null)
            {
                return NotFound($"El rol con el id {IdRole} no existe");
            }
            await _roleRepository.UpdateRole(IdRole, updateRoleDto);
            return NoContent();
        }

        [HttpDelete("{IdRole:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteRole(int IdRole)
        {
            if (!_roleRepository.DeleteRole(IdRole))
            {
                return NotFound($"El rol con el id {IdRole} no existe");
            }
            return NoContent();
        }
    }
}

