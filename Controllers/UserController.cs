using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository;
using AutoMapper;


namespace Inventory_Flow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
     public class UsersController : ControllerBase
  {
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
      _userRepository = userRepository;
      _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUsers()
    {
      var users = _userRepository.GetUsers();
      var usersDto = _mapper.Map<List<UserDto>>(users);
      return Ok(usersDto);
    }

    [HttpGet("paged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsersPaged([FromQuery] PaginationParamsDto pagination, [FromQuery] int roleId = 0)
    {
      var pagedUsers = await _userRepository.GetUsersPagedAsync(pagination.PageNumber, pagination.PageSize, pagination.Search, roleId);
      var result = new PagedResultDto<UserDto>
      {
          Items = _mapper.Map<List<UserDto>>(pagedUsers.Items),
          TotalCount = pagedUsers.TotalCount,
          PageNumber = pagedUsers.PageNumber,
          PageSize = pagedUsers.PageSize
      };
      return Ok(result);
    }

    [HttpGet("{IdUser:int}", Name = "GetUser")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUser(int IdUser)
    {
      var user = _userRepository.GetUser(IdUser);
      if (user == null)
      {
        return NotFound($"El usuario con el id {IdUser} no existe");
      }
      var userDto = _mapper.Map<UserDto>(user);
      return Ok(userDto);
    }

    [HttpPost(Name = "RegisterUser")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
    {
      if (createUserDto == null || !ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }
      if (string.IsNullOrWhiteSpace(createUserDto.UserName))
      {
        return BadRequest("Username es requerido");
      }
      if (!_userRepository.IsUniqueUser(createUserDto.UserName))
      {
        return BadRequest("El usuario ya existe");
      }
      var result = await _userRepository.Register(createUserDto);
      if (result == null)
      {
        return StatusCode(StatusCodes.Status500InternalServerError, "Error al registrar el usuario");
      }
      return CreatedAtRoute("GetUser", new { IdUser= result.IdUser }, result);
    }

    [HttpPut("{IdUser:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(int IdUser, [FromBody] UpdateUserDto updateUserDto)
    {
      if (updateUserDto == null || !ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var existing = _userRepository.GetUser(IdUser);
      if (existing == null)
      {
        return NotFound($"El usuario con el id {IdUser} no existe");
      }

      var newUserName = updateUserDto.UserName?.Trim() ?? string.Empty;
      if (string.IsNullOrWhiteSpace(newUserName))
      {
        return BadRequest("Username es requerido");
      }

      var usernameChanged = !string.Equals(existing.Username, newUserName, StringComparison.OrdinalIgnoreCase);
      if (usernameChanged && !_userRepository.IsUniqueUser(newUserName))
      {
        return BadRequest("El usuario ya existe");
      }

      await _userRepository.UpdateUser(IdUser, updateUserDto);
      return NoContent();
    }

    [HttpPost("Login", Name = "LoginUser")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
    {
      if (userLoginDto == null || !ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }
      var user = await _userRepository.Login(userLoginDto);
      if (user == null)
      {
        return Unauthorized();
      }
      return Ok(user);
    }

  }
}
