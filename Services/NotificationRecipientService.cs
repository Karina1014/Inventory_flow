using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventory_Flow.Services;

public class NotificationRecipientService : INotificationRecipientService
{
    private readonly AplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<NotificationRecipientService> _logger;

    public NotificationRecipientService(
        AplicationDbContext db,
        IConfiguration config,
        ILogger<NotificationRecipientService> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task<string> GetBodegueroPrincipalPhoneAsync()
    {
        try
        {
            // 1. Buscar usuario activo con rol "Bodeguero Principal" que tenga teléfono registrado
            var user = await _db.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .Where(u => u.IsActive && !string.IsNullOrWhiteSpace(u.Phone))
                .Where(u => u.Role != null && u.Role.Name != null && (u.Role.Name.Contains("Bodeguero Principal") || u.Role.Name.Contains("Principal")))
                .OrderByDescending(u => u.UpdatedAt)
                .FirstOrDefaultAsync();

            // 2. Si no hay Bodeguero Principal con teléfono, buscar un Administrador activo con teléfono
            if (user == null)
            {
                user = await _db.Users
                    .Include(u => u.Role)
                    .AsNoTracking()
                    .Where(u => u.IsActive && !string.IsNullOrWhiteSpace(u.Phone))
                    .Where(u => u.Role != null && (u.Role.IsAdmin || (u.Role.Name != null && (u.Role.Name == "Administrador" || u.Role.Name == "Admin"))))
                    .OrderByDescending(u => u.UpdatedAt)
                    .FirstOrDefaultAsync();
            }

            if (user != null && !string.IsNullOrWhiteSpace(user.Phone))
            {
                return CleanPhoneNumber(user.Phone);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resolver teléfono del Bodeguero Principal desde la base de datos.");
        }

        // 3. Fallback de contingencia configurado en appsettings.json
        return CleanPhoneNumber(_config["N8nSettings:MatrizRecipientPhone"] ?? "593963124396");
    }

    public async Task<string> GetBranchRecipientPhoneAsync(int idWarehouse)
    {
        try
        {
            // 1. Buscar recepcionista / bodeguero asignado a esa sucursal específica con teléfono
            var user = await _db.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .Where(u => u.IsActive && u.IdWarehouse == idWarehouse && !string.IsNullOrWhiteSpace(u.Phone))
                .OrderByDescending(u => u.Role != null && u.Role.Name != null && (u.Role.Name.Contains("Bodeguero") || u.Role.Name.Contains("Asistente")))
                .ThenByDescending(u => u.UpdatedAt)
                .FirstOrDefaultAsync();

            if (user != null && !string.IsNullOrWhiteSpace(user.Phone))
            {
                return CleanPhoneNumber(user.Phone);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resolver teléfono de la sucursal {WarehouseId} desde la base de datos.", idWarehouse);
        }

        // 2. Si esa sucursal no tiene recepcionista con celular registrado, se deriva al Bodeguero Principal de Matriz
        return await GetBodegueroPrincipalPhoneAsync();
    }

    public async Task<List<RecipientInfo>> GetRecipientsSummaryAsync()
    {
        var users = await _db.Users
            .Include(u => u.Role)
            .Include(u => u.Warehouse)
            .AsNoTracking()
            .Where(u => u.IsActive && !string.IsNullOrWhiteSpace(u.Phone))
            .OrderBy(u => u.IdWarehouse)
            .ToListAsync();

        return users.Select(u => new RecipientInfo
        {
            UserId = u.IdUser,
            Name = u.Name ?? "Sin nombre",
            Username = u.Username,
            RoleName = u.Role?.Name ?? "Sin rol",
            WarehouseId = u.IdWarehouse,
            WarehouseName = u.Warehouse?.Name ?? "Acceso Global / Matriz",
            Phone = CleanPhoneNumber(u.Phone!)
        }).ToList();
    }

    private static string CleanPhoneNumber(string rawPhone)
    {
        if (string.IsNullOrWhiteSpace(rawPhone)) return string.Empty;

        // Remover espacios, guiones y caracteres no numéricos
        var digitsOnly = Regex.Replace(rawPhone, @"[^\d]", "");

        // Si empieza con 09 (formato móvil local Ecuador), convertir a prefijo internacional 5939...
        if (digitsOnly.StartsWith("09") && digitsOnly.Length == 10)
        {
            return "593" + digitsOnly.Substring(1);
        }

        // Si ya contiene el código de país Ecuador 593
        if (digitsOnly.StartsWith("593"))
        {
            return digitsOnly;
        }

        return digitsOnly;
    }
}
