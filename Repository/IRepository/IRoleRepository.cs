using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IRoleRepository
{
    ICollection<Role> GetRoles();
    Role? GetRole(int IdRole);
    bool IsUniqueName(string name);
    Task<Role> CreateRole(CreateRoleDto createRoleDto);
    Task<Role> UpdateRole(int IdRole, CreateRoleDto updateRoleDto);
    bool DeleteRole(int IdRole);
}
