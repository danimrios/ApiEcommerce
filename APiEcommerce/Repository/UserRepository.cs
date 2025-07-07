using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APiEcommerce.Constants;
using APiEcommerce.Models;
using APiEcommerce.Models.Dtos;
using APiEcommerce.Repository.IRepository;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace APiEcommerce.Repository;

public class UserRepository : IUserRepository
{
    public readonly ApplicationDbContext  _db;
    public string? _secretKey;
    public UserRepository(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _secretKey = configuration.GetValue<String>(PolicyName.SecretKey);
        
    }
    public User? GetUser(int Id)
    {
        return _db.Users.FirstOrDefault(u => u.Id == Id);
    }

    public ICollection<User> GetUsers()
    {
        return _db.Users.OrderBy(u=>u.Name).ToList();
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
        var user = await _db.Users.FirstOrDefaultAsync<User>(u => u.Username.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());
        if (user == null) return new UserLoginResponseDto
        {
            Token = "",
            User = null,
            Message = "El username no fue encontrado"
        };
        if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
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
        var key = Encoding.UTF8.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id",user.Id.ToString()),
                new Claim("userName",user.Username),
                new Claim(ClaimTypes.Role,user.Role?? string.Empty),
            }
            ),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handlerToken.CreateToken(tokenDescriptor);
        return new UserLoginResponseDto()
            {
                Token = handlerToken.WriteToken(token),
                User = new UserRegisterDto()
                {
                    Username = user.Username,
                    Name = user.Name?? "",
                    Role = user.Role,
                    Password= user.Password?? ""
                },
                Message = "Usuario ogeado correctamente"
            };;
    }

    public async Task<User> Register(CreateUserDto createUserDto)
    {
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

    }

}
