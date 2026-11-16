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
        return _db.Users.Include(u => u.Role).Include(u => u.Warehouse).FirstOrDefault(u => u.IdUser == IdUser);
    }

    public ICollection<User> GetUsers()
    {
        return _db.Users.Include(u => u.Role).Include(u => u.Warehouse).OrderBy(u => u.Username).ToList();
    }

    public bool IsUniqueUser(string username)
    {
        return !_db.Users.Any(u => u.Username.ToLower().Trim() == username.ToLower().Trim());
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
        var user = await _db.Users
            .Include(u => u.Role)
            .Include(u => u.Warehouse)
            .FirstOrDefaultAsync<User>(u => u.Username.ToLower().Trim() == userLoginDto.UserName.ToLower().Trim());

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

        // JWT con Claims de identidad, rol y sucursal
        var handlerToken = new JwtSecurityTokenHandler();
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("SecretKey no esta configurada");
        }
        var key = Encoding.UTF8.GetBytes(secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
            new Claim("idUser", user.IdUser.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("username", user.Username),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? string.Empty)
        };

        if (user.IdWarehouse.HasValue)
        {
            claims.Add(new Claim("warehouse_id", user.IdWarehouse.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(4),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handlerToken.CreateToken(tokenDescriptor);

        return new UserLoginResponseDto()
        {
            Token = handlerToken.WriteToken(token),
            User = new UserRegisterDto()
            {
                IdUser = user.IdUser,
                UserName = user.Username,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role?.Name,
                IdWarehouse = user.IdWarehouse,
                WarehouseName = user.Warehouse?.Name,
                Password = ""
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

    public async Task<UserProfileDto?> GetProfile(int idUser)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .Include(u => u.Warehouse)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUser == idUser);

        if (user == null) return null;

        var isAdmin = string.Equals(user.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase);

        return new UserProfileDto
        {
            IdUser = user.IdUser,
            Username = user.Username,
            Name = user.Name ?? string.Empty,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role?.Name ?? "Sin Rol",
            IdWarehouse = user.IdWarehouse,
            WarehouseName = user.Warehouse != null 
                ? user.Warehouse.Name 
                : (isAdmin ? "Acceso Global (Todas las Sucursales)" : "Sin Asignar"),
            WarehouseAddress = user.Warehouse?.Address
        };
    }

    public async Task<bool> UpdateProfile(int idUser, UpdateProfileDto updateProfileDto)
    {
        var user = await _db.Users.FindAsync(idUser);
        if (user == null) return false;

        user.Name = updateProfileDto.Name;
        user.Email = updateProfileDto.Email;
        user.Phone = updateProfileDto.Phone;
        user.UpdatedAt = DateTime.Now;

        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string Message)> ChangePassword(int idUser, ChangePasswordDto changePasswordDto)
    {
        var user = await _db.Users.FindAsync(idUser);
        if (user == null)
        {
            return (false, "Usuario no encontrado.");
        }

        if (string.IsNullOrEmpty(changePasswordDto.CurrentPassword) || string.IsNullOrEmpty(changePasswordDto.NewPassword))
        {
            return (false, "Las contraseñas son requeridas.");
        }

        // 1. Verificación de la contraseña actual con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
        {
            return (false, "La contraseña actual es incorrecta.");
        }

        // 2. Prevenir reutilización de contraseña
        if (BCrypt.Net.BCrypt.Verify(changePasswordDto.NewPassword, user.Password))
        {
            return (false, "La nueva contraseña no puede ser igual a la anterior.");
        }

        // 3. Hashear nueva contraseña
        user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.Now;

        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return (true, "Contraseña actualizada exitosamente.");
    }
}
