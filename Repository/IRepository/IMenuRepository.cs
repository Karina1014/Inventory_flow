using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IMenuRepository
{
    Task<ICollection<MenuDto>> GetMenusByRoleName(string roleName);
    Task<ICollection<MenuDto>> GetMenusByRoleId(int idRole);
}
