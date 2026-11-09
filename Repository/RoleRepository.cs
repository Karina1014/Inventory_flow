using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class RoleRepository : IRoleRepository
{
    public readonly AplicationDbContext _db;

    public RoleRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public ICollection<Role> GetRoles()
    {
        return _db.Roles.OrderBy(r => r.Name).ToList();
    }

    public Role? GetRole(int IdRole)
    {
        return _db.Roles.FirstOrDefault(r => r.IdRole == IdRole);
    }

    public bool IsUniqueName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return !_db.Roles.Any(r => r.Name != null && r.Name.ToLower() == name.Trim().ToLower());
    }

    public async Task<Role> CreateRole(CreateRoleDto createRoleDto)
    {
        var role = new Role()
        {
            Name = createRoleDto.Name,
            Description = createRoleDto.Description,
            CreateDate = DateTime.UtcNow
        };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateRole(int IdRole, CreateRoleDto updateRoleDto)
    {
        var role = await _db.Roles.FindAsync(IdRole)
            ?? throw new KeyNotFoundException($"Rol con Id {IdRole} no encontrado.");

        role.Name = updateRoleDto.Name;
        role.Description = updateRoleDto.Description;

        _db.Roles.Update(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public bool DeleteRole(int IdRole)
    {
        var role = _db.Roles.FirstOrDefault(r => r.IdRole == IdRole);
        if (role == null) return false;

        _db.Roles.Remove(role);
        _db.SaveChanges();
        return true;
    }
}
