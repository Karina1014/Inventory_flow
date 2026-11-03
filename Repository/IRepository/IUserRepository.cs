using System;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository;

public interface IUserRepository
{

    ICollection<User> GetUsers();
    User? GetUser (int IdUser);
    bool IsUniqueUser(string username);
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    Task<User> Register(CreateUserDto createUserDto);

}
