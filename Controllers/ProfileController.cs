using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository;
using Inventory_Flow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserContextService _userContext;

        public ProfileController(IUserRepository userRepository, IUserContextService userContext)
        {
            _userRepository = userRepository;
            _userContext = userContext;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = _userContext.UserId;
            if (userId <= 0)
            {
                return Unauthorized("Identificador de usuario inválido en el token.");
            }

            var profile = await _userRepository.GetProfile(userId);
            if (profile == null)
            {
                return NotFound("No se encontró la información del usuario.");
            }

            return Ok(profile);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _userContext.UserId;
            if (userId <= 0)
            {
                return Unauthorized("Identificador de usuario inválido en el token.");
            }

            var success = await _userRepository.UpdateProfile(userId, dto);
            if (!success)
            {
                return NotFound("No se encontró el usuario para actualizar.");
            }

            return Ok(new { message = "Perfil actualizado exitosamente." });
        }

        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _userContext.UserId;
            if (userId <= 0)
            {
                return Unauthorized("Identificador de usuario inválido en el token.");
            }

            var (success, message) = await _userRepository.ChangePassword(userId, dto);
            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }
    }
}

