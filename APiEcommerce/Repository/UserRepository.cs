using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APiEcommerce.Constants;
using APiEcommerce.Models;
using APiEcommerce.Models.Dtos;
using APiEcommerce.Repository.IRepository;
using AutoMapper;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace APiEcommerce.Repository;

public class UserRepository : IUserRepository
{
    public readonly ApplicationDbContext  _db;
    public string? _secretKey;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;
    public UserRepository(ApplicationDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
    {
        _db = db;
        _secretKey = configuration.GetValue<String>(PolicyName.SecretKey);
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }
    public ApplicationUser? GetUser(string Id)
    {
        return _db.ApplicationUser.FirstOrDefault(u => u.Id == Id);
    }

    public ICollection<ApplicationUser> GetUsers()
    {
        return _db.ApplicationUser.OrderBy(u => u.UserName).ToList();
     
    }

    public bool IsUniqueUser(string nameUser)
    {
        return !_db.Users.Any(u => u.Username.ToLower().Trim() == nameUser.ToLower().Trim());
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Username)) return new UserLoginResponseDto
        {
            Token = "",
            User = null,
            Message = "El Username es requerido"
        };
        if (string.IsNullOrEmpty(userLoginDto.Password)) return new UserLoginResponseDto
        {
            Token = "",
            User = null,
            Message = "La contrasenia es requerido"
        };
        var user = await _db.ApplicationUser.FirstOrDefaultAsync<ApplicationUser>(u =>u.UserName!= null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());
        if (user == null) return new UserLoginResponseDto
        {
            Token = "",
            User = null,
            Message = "El username no fue encontrado"
        };
        bool IsValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);
        if (!IsValid)
        {
            return new UserLoginResponseDto
            {
                Token = "",
                User = null,
                Message = "Las cedenciales son invalidas"
            };
        }
        var handlerToken = new JwtSecurityTokenHandler();
        
        if (string.IsNullOrWhiteSpace(_secretKey)) throw new InvalidOperationException("SecretKey no esta configurado");
        var role = await _userManager.GetRolesAsync(user);
        var key = Encoding.UTF8.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id",user.Id.ToString()),
                new Claim("userName",user.UserName?? string.Empty),
                new Claim(ClaimTypes.Role,role.FirstOrDefault()?? string.Empty),
            }
            ),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handlerToken.CreateToken(tokenDescriptor);
        return new UserLoginResponseDto()
            {
                Token = handlerToken.WriteToken(token),
                User = _mapper.Map<UserDataDto >(user),
                /*User = new UserRegisterDto()
            {
                Username = user.Username,
                Name = user.Name?? "",
                Role = user.Role,
                Password= user.Password?? ""
            },*/
            Message = "Usuario ogeado correctamente"
            };;
    }

    public async Task<UserDataDto> Register(CreateUserDto createUserDto)
    {

        /*
        var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
        var User = new User()
        {
            Username = createUserDto.Username ?? "No UserName",
            Name = createUserDto.Name,
            Role = createUserDto.Role,
            Password = encriptedPassword,

        };
        _db.Users.Add(User);
        await _db.SaveChangesAsync();
        return User;
*/

        if (string.IsNullOrWhiteSpace(createUserDto.Username))
        {
            throw new ArgumentNullException("El Username es Requerido");
        }
        if (createUserDto.Password == null) throw new ArgumentNullException("El Password es Requerido");
        var user = new ApplicationUser()
        {
            UserName = createUserDto.Username,
            Email = createUserDto.Username,
            NormalizedEmail = createUserDto.Username.ToUpper(),
            Name = createUserDto.Name
        };
        var result = await _userManager.CreateAsync(user, createUserDto.Password);
        if (result.Succeeded)
        {
            var userRole = createUserDto.Role ?? "User";
            var roleExists = await _roleManager.RoleExistsAsync(userRole);
            if (!roleExists)
            {
                var identityRole = new IdentityRole(userRole);
                await _roleManager.CreateAsync(identityRole);
            }
            await _userManager.AddToRoleAsync(user, userRole);
            var createUser = _db.ApplicationUser.FirstOrDefault(u => u.UserName == createUserDto.Username);
            return _mapper.Map<UserDataDto>(createUser);
        }
        var error = string.Join(",", result.Errors.Select(e => e.Description));
        throw new ApplicationException($"No se pudo realizar el Registro: {error}");
        }

}
