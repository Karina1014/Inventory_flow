using System;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository;

public interface IUserRepository
{
    ICollection<User> GetUsers();
    Task<PagedResultDto<User>> GetUsersPagedAsync(int pageNumber, int pageSize, string? search = null, int roleId = 0);
    User? GetUser(int IdUser);
    bool IsUniqueUser(string username);
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    Task<User> Register(CreateUserDto createUserDto);
    Task<User> UpdateUser(int IdUser, UpdateUserDto updateUserDto);
    Task<UserProfileDto?> GetProfile(int idUser);
    Task<bool> UpdateProfile(int idUser, UpdateProfileDto updateProfileDto);
    Task<(bool Success, string Message)> ChangePassword(int idUser, ChangePasswordDto changePasswordDto);
}
