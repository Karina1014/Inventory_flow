using Inventory_Flow.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AplicationDbContext db)
    {
        // 1. Roles solicitados
        var rolesToSeed = new List<(string Name, string Description, bool IsAdmin)>
        {
            ("Administrador", "Acceso total y configuración global del sistema", true),
            ("Bodeguero", "Operaciones de entradas y salidas de inventario en su sucursal", false),
            ("Bodeguero Principal", "Supervisión de almacén, movimientos, lotes y umbrales", false),
            ("Asistente", "Consulta de catálogo y generación de reportes básicos", false),
            ("Financiero", "Auditoría de Kardex, análisis de costos e informes", false)
        };

        foreach (var (name, desc, isAdmin) in rolesToSeed)
        {
            var existing = await db.Roles.FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == name.ToLower());
            if (existing == null)
            {
                db.Roles.Add(new Role
                {
                    Name = name,
                    Description = desc,
                    IsAdmin = isAdmin,
                    CreateDate = DateTime.UtcNow
                });
            }
            else
            {
                existing.IsAdmin = isAdmin;
            }
        }
        await db.SaveChangesAsync();

        // 2. Traer los roles con sus IDs reales
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.IsAdmin || r.Name == "Administrador" || r.Name == "Admin");
        var bodegueroRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Bodeguero");
        var bodegueroPrincipalRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Bodeguero Principal");
        var asistenteRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Asistente");
        var financieroRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Financiero");

        // 3. Menús asignados a cada rol (verificando individualmente por rol)
        var menusToAdd = new List<Menu>();

        // Menús para ADMINISTRADOR (Acceso total)
        if (adminRole != null && !await db.Menus.AnyAsync(m => m.IdRole == adminRole.IdRole))
        {
            menusToAdd.AddRange(new[]
            {
                new Menu { Name = "Dashboard", Category = "", Url = "/dashboard", Icon = "bi bi-grid-fill", Order = 1, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Sucursales", Category = "Administración", Url = "/warehouses", Icon = "bi bi-building", Order = 2, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Proveedores", Category = "Administración", Url = "/suppliers", Icon = "bi bi-truck", Order = 3, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Categorías", Category = "Administración", Url = "/categories", Icon = "bi bi-tags-fill", Order = 4, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Productos", Category = "Administración", Url = "/products", Icon = "bi bi-box-seam", Order = 5, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Tipos de movimientos", Category = "Administración", Url = "/movementtypes", Icon = "bi bi-arrow-left-right", Order = 6, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Roles", Category = "Administración", Url = "/roles", Icon = "bi bi-shield-lock-fill", Order = 7, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Usuarios", Category = "Administración", Url = "/users", Icon = "bi bi-people-fill", Order = 8, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Umbral de Stock", Category = "Administración", Url = "/thresholds", Icon = "bi bi-sliders", Order = 9, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Control de Inventario", Category = "Inventario", Url = "/inventario", Icon = "bi bi-box-seam", Order = 10, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Requisiciones", Category = "Inventario", Url = "/requisitions", Icon = "bi bi-send-check", Order = 11, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Lotes", Category = "Inventario", Url = "/lots", Icon = "bi bi-boxes", Order = 12, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Movimientos", Category = "Inventario", Url = "/movements", Icon = "bi bi-arrow-left-right", Order = 13, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Análisis IA", Category = "Reportes", Url = "/analytics", Icon = "bi bi-stars", Order = 14, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Reporte de stock", Category = "Reportes", Url = "/stockreport", Icon = "bi bi-clipboard-data", Order = 15, IsActive = true, IdRole = adminRole.IdRole },
                new Menu { Name = "Kardex", Category = "Reportes", Url = "/kardex", Icon = "bi bi-journal-text", Order = 16, IsActive = true, IdRole = adminRole.IdRole }
            });
        }

        // Menús para BODEGUERO PRINCIPAL
        if (bodegueroPrincipalRole != null && !await db.Menus.AnyAsync(m => m.IdRole == bodegueroPrincipalRole.IdRole))
        {
            menusToAdd.AddRange(new[]
            {
                new Menu { Name = "Dashboard", Category = "", Url = "/dashboard", Icon = "bi bi-grid-fill", Order = 1, IsActive = true, IdRole = bodegueroPrincipalRole.IdRole },
                new Menu { Name = "Categorías", Category = "Catálogos", Url = "/categories", Icon = "bi bi-tags-fill", Order = 2, IsActive = true, IdRole = bodegueroPrincipalRole.IdRole },
                new Menu { Name = "Productos", Category = "Catálogos", Url = "/products", Icon = "bi bi-box-seam", Order = 3, IsActive = true, IdRole = bodegueroPrincipalRole.IdRole },
                new Menu { Name = "Lotes", Category = "Catálogos", Url = "/lots", Icon = "bi bi-boxes", Order = 4, IsActive = true, IdRole = bodegueroPrincipalRole.IdRole },
                new Menu { Name = "Umbral de Stock", Category = "Catálogos", Url = "/thresholds", Icon = "bi bi-sliders", Order = 5, IsActive = true, IdRole = bodegueroPrincipalRole.IdRole },
                new Menu { Name = "Movimientos", Category = "Operaciones", Url = "/movements", Icon = "bi bi-arrow-left-right", Order = 6, IsActive = true, IdRole = bodegueroPrincipalRole.IdRole },
                new Menu { Name = "Análisis IA", Category = "Operaciones", Url = "/analytics", Icon = "bi bi-stars", Order = 7, IsActive = true, IdRole = bodegueroPrincipalRole.IdRole },
                new Menu { Name = "Reporte de stock", Category = "Reportes", Url = "/stockreport", Icon = "bi bi-clipboard-data", Order = 8, IsActive = true, IdRole = bodegueroPrincipalRole.IdRole },
                new Menu { Name = "Kardex", Category = "Reportes", Url = "/kardex", Icon = "bi bi-journal-text", Order = 9, IsActive = true, IdRole = bodegueroPrincipalRole.IdRole }
            });
        }

        // Menús para BODEGUERO
        if (bodegueroRole != null && !await db.Menus.AnyAsync(m => m.IdRole == bodegueroRole.IdRole))
        {
            menusToAdd.AddRange(new[]
            {
                new Menu { Name = "Dashboard", Category = "", Url = "/dashboard", Icon = "bi bi-grid-fill", Order = 1, IsActive = true, IdRole = bodegueroRole.IdRole },
                new Menu { Name = "Productos", Category = "Catálogos", Url = "/products", Icon = "bi bi-box-seam", Order = 2, IsActive = true, IdRole = bodegueroRole.IdRole },
                new Menu { Name = "Lotes", Category = "Catálogos", Url = "/lots", Icon = "bi bi-boxes", Order = 3, IsActive = true, IdRole = bodegueroRole.IdRole },
                new Menu { Name = "Movimientos", Category = "Operaciones", Url = "/movements", Icon = "bi bi-arrow-left-right", Order = 4, IsActive = true, IdRole = bodegueroRole.IdRole },
                new Menu { Name = "Reporte de stock", Category = "Reportes", Url = "/stockreport", Icon = "bi bi-clipboard-data", Order = 5, IsActive = true, IdRole = bodegueroRole.IdRole },
                new Menu { Name = "Kardex", Category = "Reportes", Url = "/kardex", Icon = "bi bi-journal-text", Order = 6, IsActive = true, IdRole = bodegueroRole.IdRole }
            });
        }

        // Menús para ASISTENTE
        if (asistenteRole != null && !await db.Menus.AnyAsync(m => m.IdRole == asistenteRole.IdRole))
        {
            menusToAdd.AddRange(new[]
            {
                new Menu { Name = "Dashboard", Category = "", Url = "/dashboard", Icon = "bi bi-grid-fill", Order = 1, IsActive = true, IdRole = asistenteRole.IdRole },
                new Menu { Name = "Categorías", Category = "Catálogos", Url = "/categories", Icon = "bi bi-tags-fill", Order = 2, IsActive = true, IdRole = asistenteRole.IdRole },
                new Menu { Name = "Productos", Category = "Catálogos", Url = "/products", Icon = "bi bi-box-seam", Order = 3, IsActive = true, IdRole = asistenteRole.IdRole },
                new Menu { Name = "Reporte de stock", Category = "Reportes", Url = "/stockreport", Icon = "bi bi-clipboard-data", Order = 4, IsActive = true, IdRole = asistenteRole.IdRole }
            });
        }

        // Menús para FINANCIERO
        if (financieroRole != null && !await db.Menus.AnyAsync(m => m.IdRole == financieroRole.IdRole))
        {
            menusToAdd.AddRange(new[]
            {
                new Menu { Name = "Dashboard", Category = "", Url = "/dashboard", Icon = "bi bi-grid-fill", Order = 1, IsActive = true, IdRole = financieroRole.IdRole },
                new Menu { Name = "Movimientos", Category = "Operaciones", Url = "/movements", Icon = "bi bi-arrow-left-right", Order = 2, IsActive = true, IdRole = financieroRole.IdRole },
                new Menu { Name = "Reporte de stock", Category = "Reportes", Url = "/stockreport", Icon = "bi bi-clipboard-data", Order = 3, IsActive = true, IdRole = financieroRole.IdRole },
                new Menu { Name = "Kardex", Category = "Reportes", Url = "/kardex", Icon = "bi bi-journal-text", Order = 4, IsActive = true, IdRole = financieroRole.IdRole },
                new Menu { Name = "Análisis IA", Category = "Operaciones", Url = "/analytics", Icon = "bi bi-stars", Order = 5, IsActive = true, IdRole = financieroRole.IdRole }
            });
        }

        if (menusToAdd.Count > 0)
        {
            db.Menus.AddRange(menusToAdd);
            await db.SaveChangesAsync();
        }
    }
}
