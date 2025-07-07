using APiEcommerce.Models;
using APiEcommerce.Models.Dtos;
using APiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _UserRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _UserRepository = userRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUser()
        {
            var Users = _UserRepository.GetUsers();
            var UserDto = _mapper.Map<List<UserDto>>(Users);
            return Ok(User);
        }
        [HttpGet("{UserId:int}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUser(int UserId)
        {
            var User = _UserRepository.GetUser(UserId);
            if (User == null) return NotFound($"El Usuario con el Id {UserId} no existe");
            var userDto = _mapper.Map<UserDto>(User);
            return Ok(userDto);
        }

        [AllowAnonymous]
        [HttpPost(Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null || !ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(createUserDto.Username)) return BadRequest("El Username es requerido");
            if (string.IsNullOrWhiteSpace(createUserDto.Password)) return BadRequest("El Password es requerido");
            if (!_UserRepository.IsUniqueUser(createUserDto.Username)) return BadRequest("El Username ya existe");
            var reult = await _UserRepository.Register(createUserDto);
            if (reult == null) return StatusCode(StatusCodes.Status500InternalServerError, "Error al registrar el usuario");
            return CreatedAtRoute("GetUser", new { UserId = reult.Id }, reult);
        }

        [AllowAnonymous]
        [HttpPost("Login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
        {
            if (userLoginDto == null || !ModelState.IsValid) return BadRequest(ModelState);

            var user = await _UserRepository.Login(userLoginDto);
            if (user == null) return Unauthorized();
            return Ok(user);
        }

    }
}
