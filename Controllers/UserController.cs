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
