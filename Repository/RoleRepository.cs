using Inventory_Flow.Data;
using Inventory_Flow.Extensions;
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
        return _db.Roles
            .OrderByDescending(r => r.CreateDate)
            .ThenByDescending(r => r.IdRole)
            .ToList();
    }

    public async Task<PagedResultDto<Role>> GetRolesPagedAsync(int pageNumber, int pageSize, string? search = null, bool? isAdmin = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r =>
                (r.Name != null && r.Name.Contains(term)) ||
                (r.Description != null && r.Description.Contains(term)));
        }

        if (isAdmin.HasValue)
        {
            query = query.Where(r => r.IsAdmin == isAdmin.Value);
        }

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(r => r.CreateDate >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(r => r.CreateDate < end);
        }

        return await query
            .OrderByDescending(r => r.CreateDate)
            .ThenByDescending(r => r.IdRole)
            .ToPagedResultAsync(pageNumber, pageSize);
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
            IsAdmin = createRoleDto.IsAdmin,
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
        role.IsAdmin = updateRoleDto.IsAdmin;

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
