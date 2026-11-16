using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Inventory_Flow.Repository;

public class UserRepository : IUserRepository
{ public readonly AplicationDbContext _db; //Creamos una instancia del contexto
    private string? secretKey;
    public UserRepository(AplicationDbContext db, IConfiguration configuration)
    {
      _db = db;  
      secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
    }
    public User? GetUser(int IdUser)
    {
        return _db.Users.Include(u => u.Role).FirstOrDefault(u=> u.IdUser == IdUser);
    }

    public ICollection<User> GetUsers()
    {
        return _db.Users.Include(u => u.Role).OrderBy(u=> u.Username).ToList();
    }

    public bool IsUniqueUser(string username)
    {
        return !_db.Users.Any(u => u.Username.ToLower().Trim()== username.ToLower().Trim());
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.UserName))
        {
        return new UserLoginResponseDto()
        {
            Token = "",
            User = null,
            Message = "El Username es requerido "
        };
        }
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync<User>(u => u.Username.ToLower().Trim() == userLoginDto.UserName.ToLower().Trim());
        if (user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Username no encontrado"
            };
        }
        if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Credenciales son incorrectas"
            };
        }

         // JWT
        var handlerToken = new JwtSecurityTokenHandler ();
        if (string.IsNullOrWhiteSpace(secretKey))
            {
            throw new InvalidOperationException("SecretKey no esta configurada");
            }
        var key = Encoding.UTF8.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new  ClaimsIdentity(new[]
                {
                    new Claim("idUser",user.IdUser.ToString()),
                    new Claim("username",user.Username),
                    new Claim(ClaimTypes.Role,user.Role?.Name ?? string.Empty),
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
                    UserName = user.Username,
                    Name = user.Name,
                    Role = user.Role?.Name,
                    Password = user.Password ?? ""
                },
            Message = "Usuario logueado correctamente"
        };
    }

    public async Task<User> Register(CreateUserDto createUserDto)
    {
        var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
        
        var user = new User()
        {
            Username = createUserDto.UserName ?? "No Username",
            Name = createUserDto.Name,
            IdRole = createUserDto.IdRole,
            Password = encriptedPassword,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;

    }

    public async Task<User> UpdateUser(int IdUser, UpdateUserDto updateUserDto)
    {
        var user = await _db.Users.FindAsync(IdUser)
            ?? throw new KeyNotFoundException($"Usuario con Id {IdUser} no encontrado.");

        user.Name = updateUserDto.Name;
        user.Username = string.IsNullOrWhiteSpace(updateUserDto.UserName)
            ? user.Username
            : updateUserDto.UserName.Trim();
        user.IdRole = updateUserDto.IdRole;
        user.IsActive = updateUserDto.IsActive;

        // La contraseña solo se re-encripta si llegó una nueva
        if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
        }

        // CreatedAt no se toca: solo se marca la fecha del cambio
        user.UpdatedAt = DateTime.Now;

        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }
}
