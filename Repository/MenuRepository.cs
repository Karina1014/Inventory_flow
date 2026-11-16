using Inventory_Flow.Data;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class MenuRepository : IMenuRepository
{
    private readonly AplicationDbContext _db;

    public MenuRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<MenuDto>> GetMenusByRoleName(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return new List<MenuDto>();
        }

        var normalizedRole = roleName.Trim().ToLower();

        return await _db.Menus
            .Include(m => m.Role)
            .Where(m => m.IsActive && m.Role != null && m.Role.Name != null && m.Role.Name.ToLower() == normalizedRole)
            .OrderBy(m => m.Order)
            .Select(m => new MenuDto
            {
                IdMenu = m.IdMenu,
                Name = m.Name,
                Url = m.Url,
                Icon = m.Icon,
                Category = m.Category,
                Order = m.Order,
                IdRole = m.IdRole,
                RoleName = m.Role != null && m.Role.Name != null ? m.Role.Name : string.Empty
            })
            .ToListAsync();
    }

    public async Task<ICollection<MenuDto>> GetMenusByRoleId(int idRole)
    {
        return await _db.Menus
            .Include(m => m.Role)
            .Where(m => m.IsActive && m.IdRole == idRole)
            .OrderBy(m => m.Order)
            .Select(m => new MenuDto
            {
                IdMenu = m.IdMenu,
                Name = m.Name,
                Url = m.Url,
                Icon = m.Icon,
                Category = m.Category,
                Order = m.Order,
                IdRole = m.IdRole,
                RoleName = m.Role != null && m.Role.Name != null ? m.Role.Name : string.Empty
            })
            .ToListAsync();
    }
}
