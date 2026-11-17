using System.Text.Json;
using Inventory_Flow.Data;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class MenuRepository : IMenuRepository
{
    private readonly AplicationDbContext _db;
    private static readonly string ExtraMenusFilePath = Path.Combine(AppContext.BaseDirectory, "user_extra_menus.json");
    private static readonly SemaphoreSlim FileLock = new(1, 1);

    public MenuRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<MenuDto>> GetMenusByUserId(int idUser)
    {
        var roleMenus = await _db.Menus
            .Include(m => m.Role)
            .Where(m => m.IsActive && _db.Users.Any(u => u.IdUser == idUser && u.IdRole == m.IdRole))
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

        var extraUrls = await GetExtraMenuUrlsByUser(idUser);
        if (extraUrls.Count > 0)
        {
            var existingUrls = roleMenus
                .Select(m => m.Url.Trim().ToLower())
                .ToHashSet();

            var missingUrls = extraUrls
                .Select(u => u.Trim().ToLower())
                .Where(u => !existingUrls.Contains(u))
                .ToList();

            if (missingUrls.Count > 0)
            {
                var catalog = await GetAllDistinctMenus();
                foreach (var extra in catalog.Where(c => missingUrls.Contains(c.Url.Trim().ToLower())))
                {
                    roleMenus.Add(extra);
                }
                roleMenus = roleMenus.OrderBy(m => m.Order).ToList();
            }
        }

        return roleMenus;
    }

    public async Task<ICollection<MenuDto>> GetAllDistinctMenus()
    {
        var allMenus = await _db.Menus
            .Where(m => m.IsActive)
            .OrderBy(m => m.Order)
            .Select(m => new MenuDto
            {
                IdMenu = m.IdMenu,
                Name = m.Name,
                Url = m.Url,
                Icon = m.Icon,
                Category = m.Category,
                Order = m.Order,
                IdRole = m.IdRole
            })
            .ToListAsync();

        return allMenus
            .GroupBy(m => m.Url.Trim().ToLower())
            .Select(g => g.First())
            .OrderBy(m => m.Order)
            .ToList();
    }

    public async Task<List<string>> GetExtraMenuUrlsByUser(int idUser)
    {
        var map = await ReadExtraMenusMapAsync();
        return map.TryGetValue(idUser, out var urls) ? urls : new List<string>();
    }

    public async Task SaveExtraMenuUrlsForUser(int idUser, IEnumerable<string> urls)
    {
        var normalized = urls
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        await FileLock.WaitAsync();
        try
        {
            var map = await ReadExtraMenusMapInternalAsync();
            if (normalized.Count == 0)
            {
                map.Remove(idUser);
            }
            else
            {
                map[idUser] = normalized;
            }

            var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ExtraMenusFilePath, json);
        }
        finally
        {
            FileLock.Release();
        }
    }

    private static async Task<Dictionary<int, List<string>>> ReadExtraMenusMapAsync()
    {
        await FileLock.WaitAsync();
        try
        {
            return await ReadExtraMenusMapInternalAsync();
        }
        finally
        {
            FileLock.Release();
        }
    }

    private static async Task<Dictionary<int, List<string>>> ReadExtraMenusMapInternalAsync()
    {
        if (!File.Exists(ExtraMenusFilePath))
        {
            return new Dictionary<int, List<string>>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(ExtraMenusFilePath);
            return JsonSerializer.Deserialize<Dictionary<int, List<string>>>(json)
                   ?? new Dictionary<int, List<string>>();
        }
        catch
        {
            return new Dictionary<int, List<string>>();
        }
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
