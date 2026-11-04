using System;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository;

public class UserRepository : IUserRepository
{ public readonly AplicationDbContext _db; //Creamos una instancia del contexto
    private string? secretKey;

    public UserRepository(AplicationDbContext db)
    {
      _db = db;  
    }
    public User? GetUser(int IdUser)
    {
        return _db.Users.FirstOrDefault(u=> u.IdUser == IdUser);
    }

    public ICollection<User> GetUsers()
    {
        return _db.Users.OrderBy(u=> u.Username).ToList();
    }

    public bool IsUniqueUser(string username)
    {
        return !_db.Users.Any(u => u.Username.ToLower().Trim()== username.ToLower().Trim());
    }

    public Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        throw new NotImplementedException();
    }

    public async Task<User> Register(CreateUserDto createUserDto)
    {
        var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
        var user = new User()
        {
            Username = createUserDto.UserName ?? "No Username",
            Name = createUserDto.Name,
            Role = createUserDto.Role,
            Password = encriptedPassword
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;

    }
}
