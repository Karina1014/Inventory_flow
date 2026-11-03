using System;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository;

public class UserRepository : IUserRepository
{
    public readonly AplicationDbContext _db; //Creamos una instancia del contexto
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
        return _db.Users.OrderBy(u=> u.UserName).ToList();
    }

    public bool IsUniqueUser(string username)
    {
        return !_db.Users.Any(u => u.UserName!.ToLower().Trim()== username.ToLower().Trim());
    }

    public Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        throw new NotImplementedException();
    }

    public Task<User> Register(CreateUserDto createUserDto)
    {
        throw new NotImplementedException();
    }
}
