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

        // 3. Helper para asegurar menús individuales (sincronización idempotente)
        async Task EnsureMenuAsync(string name, string category, string url, string icon, int order, int? roleId)
        {
            if (!roleId.HasValue) return;
            var cleanUrl = "/" + url.TrimStart('/');
            var exists = await db.Menus.AnyAsync(m => m.IdRole == roleId.Value && m.Url.ToLower() == cleanUrl.ToLower());
            if (!exists)
            {
                db.Menus.Add(new Menu
                {
                    Name = name,
                    Category = category,
                    Url = cleanUrl,
                    Icon = icon,
                    Order = order,
                    IsActive = true,
                    IdRole = roleId.Value
                });
            }
        }

        // Menús para ADMINISTRADOR (Acceso total a todo el ERP)
        if (adminRole != null)
        {
            await EnsureMenuAsync("Dashboard", "", "/dashboard", "bi bi-grid-fill", 1, adminRole.IdRole);
            await EnsureMenuAsync("Sucursales", "Administración", "/warehouses", "bi bi-building", 2, adminRole.IdRole);
            await EnsureMenuAsync("Proveedores", "Administración", "/suppliers", "bi bi-truck", 3, adminRole.IdRole);
            await EnsureMenuAsync("Categorías", "Administración", "/categories", "bi bi-tags-fill", 4, adminRole.IdRole);
            await EnsureMenuAsync("Productos", "Administración", "/products", "bi bi-box-seam", 5, adminRole.IdRole);
            await EnsureMenuAsync("Tipos de movimientos", "Administración", "/movementtypes", "bi bi-arrow-left-right", 6, adminRole.IdRole);
            await EnsureMenuAsync("Roles", "Administración", "/roles", "bi bi-shield-lock-fill", 7, adminRole.IdRole);
            await EnsureMenuAsync("Usuarios", "Administración", "/users", "bi bi-people-fill", 8, adminRole.IdRole);
            await EnsureMenuAsync("Umbral de Stock", "Administración", "/thresholds", "bi bi-sliders", 9, adminRole.IdRole);
            await EnsureMenuAsync("Control de Inventario", "Inventario", "/inventario", "bi bi-box-seam", 10, adminRole.IdRole);
            await EnsureMenuAsync("Requisiciones", "Inventario", "/requisitions", "bi bi-send-check", 11, adminRole.IdRole);
            await EnsureMenuAsync("Órdenes de Compra", "Inventario", "/purchase-orders", "bi bi-cart-check", 12, adminRole.IdRole);
            await EnsureMenuAsync("Lotes", "Inventario", "/lots", "bi bi-boxes", 13, adminRole.IdRole);
            await EnsureMenuAsync("Movimientos", "Inventario", "/movements", "bi bi-arrow-left-right", 14, adminRole.IdRole);
            await EnsureMenuAsync("Análisis IA", "Reportes", "/analytics", "bi bi-stars", 15, adminRole.IdRole);
            await EnsureMenuAsync("Reporte de stock", "Reportes", "/stockreport", "bi bi-clipboard-data", 16, adminRole.IdRole);
            await EnsureMenuAsync("Kardex", "Reportes", "/kardex", "bi bi-journal-text", 17, adminRole.IdRole);
        }

        // Menús para BODEGUERO PRINCIPAL (Gestión de CEDI Carapungo y despachos)
        if (bodegueroPrincipalRole != null)
        {
            await EnsureMenuAsync("Dashboard", "", "/dashboard", "bi bi-grid-fill", 1, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Control de Inventario", "Operaciones", "/inventario", "bi bi-box-seam", 2, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Requisiciones", "Operaciones", "/requisitions", "bi bi-send-check", 3, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Lotes", "Operaciones", "/lots", "bi bi-boxes", 4, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Movimientos", "Operaciones", "/movements", "bi bi-arrow-left-right", 5, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Productos", "Catálogos", "/products", "bi bi-box-seam", 6, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Proveedores", "Catálogos", "/suppliers", "bi bi-truck", 7, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Reporte de stock", "Reportes", "/stockreport", "bi bi-clipboard-data", 8, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Kardex", "Reportes", "/kardex", "bi bi-journal-text", 9, bodegueroPrincipalRole.IdRole);
        }

        // Menús para BODEGUERO SUCURSAL
        if (bodegueroRole != null)
        {
            await EnsureMenuAsync("Dashboard", "", "/dashboard", "bi bi-grid-fill", 1, bodegueroRole.IdRole);
            await EnsureMenuAsync("Control de Inventario", "Operaciones", "/inventario", "bi bi-box-seam", 2, bodegueroRole.IdRole);
            await EnsureMenuAsync("Requisiciones", "Operaciones", "/requisitions", "bi bi-send-check", 3, bodegueroRole.IdRole);
            await EnsureMenuAsync("Movimientos", "Operaciones", "/movements", "bi bi-arrow-left-right", 4, bodegueroRole.IdRole);
            await EnsureMenuAsync("Lotes", "Catálogos", "/lots", "bi bi-boxes", 5, bodegueroRole.IdRole);
            await EnsureMenuAsync("Reporte de stock", "Reportes", "/stockreport", "bi bi-clipboard-data", 6, bodegueroRole.IdRole);
        }

        // Menús para FINANCIERO (Auditoría de Kardex y Reportes)
        if (financieroRole != null)
        {
            await EnsureMenuAsync("Dashboard", "", "/dashboard", "bi bi-grid-fill", 1, financieroRole.IdRole);
            await EnsureMenuAsync("Kardex", "Finanzas", "/kardex", "bi bi-journal-text", 2, financieroRole.IdRole);
            await EnsureMenuAsync("Reporte de stock", "Reportes", "/stockreport", "bi bi-clipboard-data", 3, financieroRole.IdRole);
            await EnsureMenuAsync("Análisis IA", "Reportes", "/analytics", "bi bi-stars", 4, financieroRole.IdRole);
        }

        // Asegurar que Órdenes de Compra pertenezca ÚNICAMENTE al rol Administrador en la BD
        if (adminRole != null)
        {
            var nonAdminOrderMenus = await db.Menus
                .Where(m => m.Url.Contains("purchase-orders") && m.IdRole != adminRole.IdRole)
                .ToListAsync();

            if (nonAdminOrderMenus.Any())
            {
                db.Menus.RemoveRange(nonAdminOrderMenus);
            }
        }

        await db.SaveChangesAsync();

        // 4. Semilla de Proveedores Farmacéuticos Veterinarios si no existen
        if (!await db.Suppliers.AnyAsync())
        {
            db.Suppliers.AddRange(new[]
            {
                new Supplier
                {
                    Ruc = "1790012345001",
                    CompanyName = "Laboratorios Bayer Animal Health Ecuador",
                    ContactName = "Dr. Carlos Andrade",
                    Phone = "0991234567",
                    Email = "pedidos@bayer-vet.ec",
                    Address = "Av. Simón Bolívar y Panamericana Norte, Quito",
                    DeliveryLeadTimeDays = 2,
                    IsActive = true
                },
                new Supplier
                {
                    Ruc = "1791234567001",
                    CompanyName = "Zoetis Ecuador Cía. Ltda.",
                    ContactName = "Ing. Mariana Torres",
                    Phone = "0987654321",
                    Email = "ventas@zoetis.ec",
                    Address = "Parque Industrial Sur, Lote 14, Quito",
                    DeliveryLeadTimeDays = 3,
                    IsActive = true
                },
                new Supplier
                {
                    Ruc = "1792345678001",
                    CompanyName = "Boehringer Ingelheim Animal Health",
                    ContactName = "Dra. Valeria Salazar",
                    Phone = "0976543210",
                    Email = "contacto@boehringer-vet.ec",
                    Address = "Av. de los Granados y 6 de Diciembre, Quito",
                    DeliveryLeadTimeDays = 5,
                    IsActive = true
                }
            });
            await db.SaveChangesAsync();
        }

        // 4.1 Semilla de Bodegas (Matriz Carapungo + Sucursales) si no existen
        if (!await db.Warehouses.AnyAsync())
        {
            db.Warehouses.AddRange(new[]
            {
                new Warehouse
                {
                    Name = "Bodega Matriz Carapungo",
                    Address = "Av. Panamericana Norte y Galo Plaza, Carapungo, Quito",
                    IsActive = true,
                    CreationDate = DateTime.Now
                },
                new Warehouse
                {
                    Name = "Sucursal Llano Grande",
                    Address = "Av. 23 de Abril y García Moreno, Llano Grande, Quito",
                    IsActive = true,
                    CreationDate = DateTime.Now
                },
                new Warehouse
                {
                    Name = "Sucursal Pusuquí",
                    Address = "Av. Manuel Córdova Galarza Km 12, Pusuquí, Quito",
                    IsActive = true,
                    CreationDate = DateTime.Now
                }
            });
            await db.SaveChangesAsync();
        }

        // 4.2 Semilla de Categorías Veterinarias si no existen
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(new[]
            {
                new Category { Name = "Fármacos y Antibióticos", Description = "Antibióticos inyectables y comprimidos para caninos y felinos" },
                new Category { Name = "Inmunológicos y Vacunas", Description = "Biológicos y vacunas de cadena de frío veterinaria" },
                new Category { Name = "Antiinflamatorios y Analgésicos", Description = "AINEs y analgésicos para control del dolor animal" },
                new Category { Name = "Antiparasitarios y Pipetas", Description = "Desparasitantes internos y externos de amplio espectro" },
                new Category { Name = "Suplementos y Nutracéuticos", Description = "Vitaminas, minerales y condroprotectores" }
            });
            await db.SaveChangesAsync();
        }

        // 4.3 Sincronización oficial de los 3 Tipos de Movimiento: Ingreso, Egreso y Traslado
        var currentMovementTypes = await db.MovementTypes.ToListAsync();

        // 1. Ingreso
        var ingresoType = currentMovementTypes.FirstOrDefault(m => 
            m.Mode.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) || 
            m.Mode.Equals("E", StringComparison.OrdinalIgnoreCase) || 
            m.Code == "ING" || m.Code == "01");

        if (ingresoType == null)
        {
            ingresoType = new MovementType { Code = "01", Mode = "Ingreso", Description = "Ingreso de mercadería / inventario" };
            db.MovementTypes.Add(ingresoType);
        }
        else
        {
            ingresoType.Mode = "Ingreso";
            ingresoType.Description = "Ingreso de mercadería / inventario";
        }

        // 2. Egreso
        var egresoType = currentMovementTypes.FirstOrDefault(m => 
            m.Mode.Equals("Egreso", StringComparison.OrdinalIgnoreCase) || 
            m.Mode.Equals("S", StringComparison.OrdinalIgnoreCase) || 
            m.Code == "EGR" || m.Code == "02");

        if (egresoType == null)
        {
            egresoType = new MovementType { Code = "02", Mode = "Egreso", Description = "Egreso por venta o consumo" };
            db.MovementTypes.Add(egresoType);
        }
        else
        {
            egresoType.Mode = "Egreso";
            egresoType.Description = "Egreso por venta o consumo";
        }

        // 3. Traslado
        var trasladoType = currentMovementTypes.FirstOrDefault(m => 
            m.Mode.Equals("Traslado", StringComparison.OrdinalIgnoreCase) || 
            m.Mode.Equals("T", StringComparison.OrdinalIgnoreCase) || 
            m.Code == "TRA" || m.Code == "03");

        if (trasladoType == null)
        {
            trasladoType = new MovementType { Code = "03", Mode = "Traslado", Description = "Traslado entre sucursales" };
            db.MovementTypes.Add(trasladoType);
        }
        else
        {
            trasladoType.Mode = "Traslado";
            trasladoType.Description = "Traslado entre sucursales";
        }

        await db.SaveChangesAsync();

        // 4.4 Semilla de Productos Farmacéuticos Veterinarios si no existen
        if (!await db.Products.AnyAsync())
        {
            var cats = await db.Categories.ToListAsync();
            var farmacosCat = cats.FirstOrDefault(c => c.Name.Contains("Fármacos")) ?? cats.First();
            var vacunasCat = cats.FirstOrDefault(c => c.Name.Contains("Inmunológicos")) ?? cats.First();
            var ainesCat = cats.FirstOrDefault(c => c.Name.Contains("Antiinflamatorios")) ?? cats.First();
            var antiparasitariosCat = cats.FirstOrDefault(c => c.Name.Contains("Antiparasitarios")) ?? cats.First();
            var suplementosCat = cats.FirstOrDefault(c => c.Name.Contains("Suplementos")) ?? cats.First();

            db.Products.AddRange(new[]
            {
                new Product { Code = "ATB002", Name = "Amoxicilina + ác. clavulánico 250mg", Description = "Antibiótico de amplio espectro para caninos y felinos", SuggestedPrice = 14.50m, Iva = true, IsActive = true, IdCategory = farmacosCat.IdCategory },
                new Product { Code = "ATB003", Name = "Cefalexina 500mg Comprimidos", Description = "Tratamiento de infecciones dermatológicas bacterianas", SuggestedPrice = 12.00m, Iva = true, IsActive = true, IdCategory = farmacosCat.IdCategory },
                new Product { Code = "ATB004", Name = "Enrofloxacina Inyectable 100ml", Description = "Fluoroquinolona veterinaria de amplio espectro", SuggestedPrice = 22.50m, Iva = true, IsActive = true, IdCategory = farmacosCat.IdCategory },
                new Product { Code = "VAC001", Name = "Vacuna Séxtuple Canina Nobivac", Description = "Inmunización contra parvovirus, moquillo, hepatitis y leptospira", SuggestedPrice = 18.00m, Iva = false, IsActive = true, IdCategory = vacunasCat.IdCategory },
                new Product { Code = "VAC002", Name = "Vacuna Antirrábica Nobivac Rabies", Description = "Vacuna monovalente inactivada contra el virus de la rabia", SuggestedPrice = 8.50m, Iva = false, IsActive = true, IdCategory = vacunasCat.IdCategory },
                new Product { Code = "VAC003", Name = "Vacuna Triple Felina Felocell", Description = "Protección contra rinotraqueitis, calicivirus y panleucopenia", SuggestedPrice = 16.50m, Iva = false, IsActive = true, IdCategory = vacunasCat.IdCategory },
                new Product { Code = "AIN001", Name = "Meloxicam Gotas 10ml (Meloxivet)", Description = "Antiinflamatorio no esteroideo para analgesia postoperatoria", SuggestedPrice = 9.80m, Iva = true, IsActive = true, IdCategory = ainesCat.IdCategory },
                new Product { Code = "AIN002", Name = "Carprofeno 100mg Comprimidos", Description = "Alivio del dolor osteoarticular y de columna en perros", SuggestedPrice = 24.00m, Iva = true, IsActive = true, IdCategory = ainesCat.IdCategory },
                new Product { Code = "AIN003", Name = "Tramadol Gotas 20ml", Description = "Analgésico opioide para dolor agudo y moderado", SuggestedPrice = 11.20m, Iva = true, IsActive = true, IdCategory = ainesCat.IdCategory },
                new Product { Code = "ANT001", Name = "Bravecto Perros 10-20kg (1 Comp)", Description = "Protección contra pulgas y garrapatas por 12 semanas", SuggestedPrice = 34.00m, Iva = true, IsActive = true, IdCategory = antiparasitariosCat.IdCategory },
                new Product { Code = "ANT002", Name = "Nexgard Spectra Antiparasitario M", Description = "Tableta masticable sabor a carne para pulgas, garrapatas y parásitos", SuggestedPrice = 28.50m, Iva = true, IsActive = true, IdCategory = antiparasitariosCat.IdCategory },
                new Product { Code = "ANT003", Name = "Ivermectina 1% Inyectable 50ml", Description = "Endectocida antiparasitario interno y externo", SuggestedPrice = 13.00m, Iva = true, IsActive = true, IdCategory = antiparasitariosCat.IdCategory },
                new Product { Code = "SUP001", Name = "Suplemento Vitamínico Nutri-Plus Gel", Description = "Pasta oral de alta energía para animales convalecientes", SuggestedPrice = 16.00m, Iva = true, IsActive = true, IdCategory = suplementosCat.IdCategory },
                new Product { Code = "SUP002", Name = "Condroprotector Artroglycan 30 Comp", Description = "Regenerador articular con glucosamina y condroitina", SuggestedPrice = 26.00m, Iva = true, IsActive = true, IdCategory = suplementosCat.IdCategory },
                new Product { Code = "SUP003", Name = "Complejo B + Calcio Inyectable 100ml", Description = "Estimulante metabólico y reconstituyente vitamínico", SuggestedPrice = 15.20m, Iva = true, IsActive = true, IdCategory = suplementosCat.IdCategory }
            });
            await db.SaveChangesAsync();
        }

        // 5. Identificar Bodega Matriz (Carapungo) y Sucursales Periféricas
        var allWarehouses = await db.Warehouses.Where(w => w.IsActive).OrderBy(w => w.IdWarehouse).ToListAsync();
        var matrizWarehouse = allWarehouses.FirstOrDefault(w =>
            w.Name.Contains("Carapungo", StringComparison.OrdinalIgnoreCase) ||
            w.Name.Contains("Matriz", StringComparison.OrdinalIgnoreCase) ||
            w.Name.Contains("Principal", StringComparison.OrdinalIgnoreCase) ||
            (w.Address != null && w.Address.Contains("Carapungo", StringComparison.OrdinalIgnoreCase)))
            ?? allWarehouses.FirstOrDefault();

        var branchWarehouses = allWarehouses
            .Where(w => matrizWarehouse == null || w.IdWarehouse != matrizWarehouse.IdWarehouse)
            .ToList();

        var branch1 = branchWarehouses.FirstOrDefault() ?? matrizWarehouse;
        var branch2 = branchWarehouses.Skip(1).FirstOrDefault() ?? branch1;
        var adminUser = await db.Users.FirstOrDefaultAsync() ?? new User { IdUser = 1 };
        var activeProducts = await db.Products.Where(p => p.IsActive).OrderBy(p => p.IdProduct).ToListAsync();

        // 6. Semilla de Lotes FEFO realistas en Matriz Carapungo y Sucursales
        if (matrizWarehouse != null && activeProducts.Count >= 5)
        {
            var activeLotsInMatriz = await db.Lots.CountAsync(l => l.IdWarehouse == matrizWarehouse.IdWarehouse && l.IsActive && l.Quantity > 0);
            if (activeLotsInMatriz < 8)
            {
                var seedLots = new List<Lot>();
                int prodCount = Math.Min(20, activeProducts.Count);

                for (int i = 0; i < prodCount; i++)
                {
                    var prod = activeProducts[i];

                    // Lote principal en Matriz Carapungo con buen stock para despachar requisiciones
                    DateTime expDate;
                    int qty;
                    string prefix = i % 3 == 0 ? "LOT-BAY" : (i % 3 == 1 ? "LOT-ZOE" : "LOT-BOE");

                    if (i == 0)
                    {
                        // Lote próximo a vencer (12 días) para alerta sanitaria
                        expDate = DateTime.Today.AddDays(12);
                        qty = 18;
                    }
                    else if (i == 1)
                    {
                        // Lote próximo a vencer (22 días)
                        expDate = DateTime.Today.AddDays(22);
                        qty = 25;
                    }
                    else if (i == 2)
                    {
                        // Lote vencido en cuarentena (-8 días)
                        expDate = DateTime.Today.AddDays(-8);
                        qty = 6;
                    }
                    else if (i == 3 || i == 4)
                    {
                        // Bajo stock en Matriz (3 unidades)
                        expDate = DateTime.Today.AddMonths(14);
                        qty = 3;
                    }
                    else
                    {
                        // Lote vigente con stock abundante (45 a 120 unidades)
                        expDate = DateTime.Today.AddMonths(8 + (i % 16));
                        qty = 45 + (i * 4);
                    }

                    seedLots.Add(new Lot
                    {
                        IdProduct = prod.IdProduct,
                        IdWarehouse = matrizWarehouse.IdWarehouse,
                        LotNumber = $"{prefix}-2026-{(i + 1):D3}",
                        Quantity = qty,
                        ExpirationDate = expDate,
                        CreationDate = DateTime.Now.AddDays(-15),
                        IsActive = true
                    });

                    // Si el lote 2 estaba vencido, agregar un segundo lote vigente del mismo producto para poder despachar
                    if (i <= 4)
                    {
                        seedLots.Add(new Lot
                        {
                            IdProduct = prod.IdProduct,
                            IdWarehouse = matrizWarehouse.IdWarehouse,
                            LotNumber = $"{prefix}-FEFO-{(i + 10):D3}",
                            Quantity = 60,
                            ExpirationDate = DateTime.Today.AddMonths(18),
                            CreationDate = DateTime.Now.AddDays(-5),
                            IsActive = true
                        });
                    }
                }

                // Sembrar lotes en Sucursal 1 (Llano Grande) con mezcla de stock normal y bajo stock
                if (branch1 != null && branch1.IdWarehouse != matrizWarehouse.IdWarehouse)
                {
                    for (int i = 0; i < Math.Min(12, activeProducts.Count); i++)
                    {
                        var prod = activeProducts[i];
                        int branchQty = (i % 3 == 0) ? 2 : ((i % 3 == 1) ? 4 : 18); // 2 y 4 están bajo umbral (5)
                        seedLots.Add(new Lot
                        {
                            IdProduct = prod.IdProduct,
                            IdWarehouse = branch1.IdWarehouse,
                            LotNumber = $"LOT-SUC1-{(i + 1):D3}",
                            Quantity = branchQty,
                            ExpirationDate = DateTime.Today.AddMonths(10 + i),
                            CreationDate = DateTime.Now.AddDays(-10),
                            IsActive = true
                        });
                    }
                }

                // Sembrar lotes en Sucursal 2 (Pusuquí) con mezcla de stock normal y bajo stock
                if (branch2 != null && branch2.IdWarehouse != matrizWarehouse.IdWarehouse && branch2.IdWarehouse != branch1?.IdWarehouse)
                {
                    for (int i = 0; i < Math.Min(10, activeProducts.Count); i++)
                    {
                        var prod = activeProducts[i];
                        int branchQty = (i % 2 == 0) ? 3 : 22; // 3 está bajo umbral (5)
                        seedLots.Add(new Lot
                        {
                            IdProduct = prod.IdProduct,
                            IdWarehouse = branch2.IdWarehouse,
                            LotNumber = $"LOT-SUC2-{(i + 1):D3}",
                            Quantity = branchQty,
                            ExpirationDate = DateTime.Today.AddMonths(12 + i),
                            CreationDate = DateTime.Now.AddDays(-8),
                            IsActive = true
                        });
                    }
                }

                db.Lots.AddRange(seedLots);
                await db.SaveChangesAsync();
            }
        }

        // 6.1 Sincronización y Semilla Integral para todos los productos del catálogo
        if (matrizWarehouse != null && activeProducts.Count > 0)
        {
            var existingProductIdsInMatriz = await db.Lots
                .Where(l => l.IdWarehouse == matrizWarehouse.IdWarehouse && l.IsActive && l.Quantity > 0)
                .Select(l => l.IdProduct)
                .Distinct()
                .ToListAsync();

            var productsWithoutLots = activeProducts
                .Where(p => !existingProductIdsInMatriz.Contains(p.IdProduct))
                .ToList();

            if (productsWithoutLots.Count > 0)
            {
                // Dejamos exactamente los últimos 2 o 3 productos intencionalmente AGOTADOS (sin stock)
                // para simular casos reales de quiebre de stock para la tesis (alertas n8n y órdenes de compra)
                int depletedToKeep = Math.Min(3, productsWithoutLots.Count);
                var productsToSeed = productsWithoutLots.Take(productsWithoutLots.Count - depletedToKeep).ToList();

                var newLots = new List<Lot>();
                int seedIndex = 1;

                foreach (var prod in productsToSeed)
                {
                    string prefix = (seedIndex % 3 == 0) ? "LOT-VET" : ((seedIndex % 3 == 1) ? "LOT-MED" : "LOT-CEDI");

                    // 2 productos con bajo stock intencional (2-3 unidades) para pruebas de umbral
                    if (seedIndex <= 2)
                    {
                        newLots.Add(new Lot
                        {
                            IdProduct = prod.IdProduct,
                            IdWarehouse = matrizWarehouse.IdWarehouse,
                            LotNumber = $"{prefix}-2026-{seedIndex:D3}",
                            Quantity = 3,
                            ExpirationDate = DateTime.Today.AddMonths(14),
                            CreationDate = DateTime.Now.AddDays(-10),
                            IsActive = true
                        });
                    }
                    else if (seedIndex == 3)
                    {
                        // 1 producto con alerta próxima a vencer (20 días)
                        newLots.Add(new Lot
                        {
                            IdProduct = prod.IdProduct,
                            IdWarehouse = matrizWarehouse.IdWarehouse,
                            LotNumber = $"{prefix}-ALERTA-{seedIndex:D3}",
                            Quantity = 25,
                            ExpirationDate = DateTime.Today.AddDays(20),
                            CreationDate = DateTime.Now.AddDays(-20),
                            IsActive = true
                        });
                    }
                    else
                    {
                        // Stock óptimo y saludable en Matriz Carapungo (35 a 95 unidades)
                        int qty = 35 + ((seedIndex * 7) % 55);
                        newLots.Add(new Lot
                        {
                            IdProduct = prod.IdProduct,
                            IdWarehouse = matrizWarehouse.IdWarehouse,
                            LotNumber = $"{prefix}-2026-{seedIndex:D3}",
                            Quantity = qty,
                            ExpirationDate = DateTime.Today.AddMonths(8 + (seedIndex % 18)),
                            CreationDate = DateTime.Now.AddDays(-15),
                            IsActive = true
                        });
                    }

                    // Sembrar stock proporcional en sucursales periféricas
                    if (branch1 != null && branch1.IdWarehouse != matrizWarehouse.IdWarehouse && seedIndex % 2 == 0)
                    {
                        newLots.Add(new Lot
                        {
                            IdProduct = prod.IdProduct,
                            IdWarehouse = branch1.IdWarehouse,
                            LotNumber = $"LOT-SUC1-{seedIndex:D3}",
                            Quantity = 10 + (seedIndex % 15),
                            ExpirationDate = DateTime.Today.AddMonths(12 + (seedIndex % 12)),
                            CreationDate = DateTime.Now.AddDays(-10),
                            IsActive = true
                        });
                    }

                    if (branch2 != null && branch2.IdWarehouse != matrizWarehouse.IdWarehouse && branch2.IdWarehouse != branch1?.IdWarehouse && seedIndex % 3 == 0)
                    {
                        newLots.Add(new Lot
                        {
                            IdProduct = prod.IdProduct,
                            IdWarehouse = branch2.IdWarehouse,
                            LotNumber = $"LOT-SUC2-{seedIndex:D3}",
                            Quantity = 8 + (seedIndex % 12),
                            ExpirationDate = DateTime.Today.AddMonths(14 + (seedIndex % 10)),
                            CreationDate = DateTime.Now.AddDays(-8),
                            IsActive = true
                        });
                    }

                    seedIndex++;
                }

                if (newLots.Count > 0)
                {
                    db.Lots.AddRange(newLots);
                    await db.SaveChangesAsync();
                }
            }
        }

        // 7. Semilla de Órdenes de Compra Demostrativas
        if (!await db.PurchaseOrders.AnyAsync())
        {
            var firstSupplier = await db.Suppliers.FirstOrDefaultAsync();
            var secondSupplier = await db.Suppliers.Skip(1).FirstOrDefaultAsync() ?? firstSupplier;
            var thirdSupplier = await db.Suppliers.Skip(2).FirstOrDefaultAsync() ?? firstSupplier;

            if (firstSupplier != null && matrizWarehouse != null && activeProducts.Count >= 4)
            {
                var p1 = activeProducts[0];
                var p2 = activeProducts[1];
                var p3 = activeProducts[2];
                var p4 = activeProducts[3];

                // Orden 1: Pendiente de Aprobación Financiera
                var order1 = new PurchaseOrder
                {
                    OrderNumber = "OC-2026-0001",
                    OrderDate = DateTime.Now.AddHours(-4),
                    IdSupplier = firstSupplier.IdSupplier,
                    IdDestinationWarehouse = matrizWarehouse.IdWarehouse,
                    IdUser = adminUser.IdUser,
                    IdPurchaseOrderType = 1, // Desabastecimiento Crítico
                    IdPurchaseOrderStatus = 2, // PENDIENTE_FINANCIERO
                    ExpectedDeliveryDate = DateTime.Now.AddDays(2),
                    Observation = "Reabastecimiento urgente de antibióticos y antiinflamatorios para quirófano central Carapungo.",
                    Subtotal = 690.00m,
                    TaxAmount = 103.50m,
                    TotalAmount = 793.50m
                };
                order1.Details.Add(new PurchaseOrderDetail
                {
                    IdProduct = p1.IdProduct,
                    Quantity = 30,
                    ReceivedQuantity = 0,
                    UnitPrice = 15.00m,
                    Subtotal = 450.00m,
                    TaxPercentage = 15.00m,
                    TaxAmount = 67.50m,
                    Total = 517.50m,
                    Notes = "Lote con vencimiento mínimo 18 meses"
                });
                order1.Details.Add(new PurchaseOrderDetail
                {
                    IdProduct = p2.IdProduct,
                    Quantity = 40,
                    ReceivedQuantity = 0,
                    UnitPrice = 6.00m,
                    Subtotal = 240.00m,
                    TaxPercentage = 15.00m,
                    TaxAmount = 36.00m,
                    Total = 276.00m,
                    Notes = "Presentación hospitalaria"
                });

                // Orden 2: Aprobada por Finanzas (Lista para Recepcionar Mercadería con Lotes FEFO)
                var order2 = new PurchaseOrder
                {
                    OrderNumber = "OC-2026-0002",
                    OrderDate = DateTime.Now.AddDays(-2),
                    IdSupplier = secondSupplier?.IdSupplier ?? firstSupplier.IdSupplier,
                    IdDestinationWarehouse = matrizWarehouse.IdWarehouse,
                    IdUser = adminUser.IdUser,
                    IdPurchaseOrderType = 2, // Reposición Periódica CEDI
                    IdPurchaseOrderStatus = 3, // APROBADO_FINANCIERO
                    ExpectedDeliveryDate = DateTime.Now.AddDays(1),
                    Observation = "Reposición mensual planificada de fármacos veterinarios y antiparasitarios.",
                    Subtotal = 1200.00m,
                    TaxAmount = 180.00m,
                    TotalAmount = 1380.00m,
                    FinancialApprovalDate = DateTime.Now.AddDays(-1),
                    IdFinancialApprovalUser = adminUser.IdUser,
                    FinancialApprovalNote = "Aprobado por Dirección Financiera conforme al presupuesto operativo del mes."
                };
                order2.Details.Add(new PurchaseOrderDetail
                {
                    IdProduct = p3.IdProduct,
                    Quantity = 80,
                    ReceivedQuantity = 0,
                    UnitPrice = 10.00m,
                    Subtotal = 800.00m,
                    TaxPercentage = 15.00m,
                    TaxAmount = 120.00m,
                    Total = 920.00m,
                    Notes = "Mantener cadena de frío 2-8°C durante transporte"
                });
                order2.Details.Add(new PurchaseOrderDetail
                {
                    IdProduct = p4.IdProduct,
                    Quantity = 50,
                    ReceivedQuantity = 0,
                    UnitPrice = 8.00m,
                    Subtotal = 400.00m,
                    TaxPercentage = 15.00m,
                    TaxAmount = 60.00m,
                    Total = 460.00m,
                    Notes = "Entrega en CEDI Carapungo"
                });

                // Orden 3: Borrador
                var order3 = new PurchaseOrder
                {
                    OrderNumber = "OC-2026-0003",
                    OrderDate = DateTime.Now.AddMinutes(-45),
                    IdSupplier = thirdSupplier?.IdSupplier ?? firstSupplier.IdSupplier,
                    IdDestinationWarehouse = matrizWarehouse.IdWarehouse,
                    IdUser = adminUser.IdUser,
                    IdPurchaseOrderType = 4, // Bajo Pedido Especial
                    IdPurchaseOrderStatus = 1, // BORRADOR
                    ExpectedDeliveryDate = DateTime.Now.AddDays(5),
                    Observation = "Borrador de insumos quirúrgicos especiales para traumatología veterinaria.",
                    Subtotal = 280.00m,
                    TaxAmount = 42.00m,
                    TotalAmount = 322.00m
                };
                order3.Details.Add(new PurchaseOrderDetail
                {
                    IdProduct = p1.IdProduct,
                    Quantity = 20,
                    ReceivedQuantity = 0,
                    UnitPrice = 14.00m,
                    Subtotal = 280.00m,
                    TaxPercentage = 15.00m,
                    TaxAmount = 42.00m,
                    Total = 322.00m,
                    Notes = "Cotización referencial"
                });

                db.PurchaseOrders.AddRange(order1, order2, order3);
                await db.SaveChangesAsync();
            }
        }

        // 8. Semilla Completa de Requisiciones Internas (Sucursales -> Matriz Carapungo)
        if (!await db.InternalRequisitions.AnyAsync() && matrizWarehouse != null && branch1 != null && activeProducts.Count >= 6)
        {
            var p1 = activeProducts[1]; // Amoxicilina 500 mg
            var p2 = activeProducts[2]; // Amoxicilina + ác. clavulánico
            var p3 = activeProducts[3]; // Enrofloxacina 50 mg
            var p4 = activeProducts[4]; // Doxiciclina 100 mg
            var p5 = activeProducts[5]; // Metronidazol 250 mg
            var p6 = activeProducts[6]; // Meloxicam inyectable

            // Requisición 1: SOLICITADO (Urgencia Quirúrgica desde Llano Grande -> Lista para Despachar en Carapungo)
            var req1 = new InternalRequisition
            {
                RequisitionCode = "REQ-2026-0001",
                RequisitionDate = DateTime.Now.AddHours(-3),
                IdRequestingWarehouse = branch1.IdWarehouse,
                IdFulfillingWarehouse = matrizWarehouse.IdWarehouse,
                IdUser = adminUser.IdUser,
                IdRequisitionType = 1, // Urgencia quirúrgica / emergencia (SLA: 4 h)
                IdRequisitionStatus = 1, // SOLICITADO
                Observation = "Urgencia quirúrgica en clínica Llano Grande: requerimos antibióticos y analgésicos inyectables para cirugías programadas."
            };
            req1.Details.Add(new InternalRequisitionDetail
            {
                IdProduct = p2.IdProduct,
                RequestedQuantity = 12,
                DispatchedQuantity = 0,
                Note = "Prioridad quirófano"
            });
            req1.Details.Add(new InternalRequisitionDetail
            {
                IdProduct = p6.IdProduct,
                RequestedQuantity = 10,
                DispatchedQuantity = 0,
                Note = "Uso post-operatorio"
            });

            // Requisición 2: EN_REVISION (Reposición Ordinaria desde Pusuquí -> En preparación en CEDI Carapungo)
            var req2 = new InternalRequisition
            {
                RequisitionCode = "REQ-2026-0002",
                RequisitionDate = DateTime.Now.AddHours(-14),
                IdRequestingWarehouse = (branch2 ?? branch1).IdWarehouse,
                IdFulfillingWarehouse = matrizWarehouse.IdWarehouse,
                IdUser = adminUser.IdUser,
                IdRequisitionType = 3, // Reposición ordinaria de stock (SLA: 48 h)
                IdRequisitionStatus = 2, // EN_REVISION
                Observation = "Reposición semanal de antibióticos de amplio espectro para farmacia de Pusuquí.",
                ReviewedDate = DateTime.Now.AddHours(-2),
                IdReviewedByUser = adminUser.IdUser,
                ReviewNote = "Verificando lotes FEFO en bodega principal Carapungo."
            };
            req2.Details.Add(new InternalRequisitionDetail
            {
                IdProduct = p3.IdProduct,
                RequestedQuantity = 20,
                DispatchedQuantity = 0,
                Note = "Reposición mostrador"
            });
            req2.Details.Add(new InternalRequisitionDetail
            {
                IdProduct = p4.IdProduct,
                RequestedQuantity = 15,
                DispatchedQuantity = 0,
                Note = "Stock de seguridad"
            });

            // Requisición 3: DESPACHADO / EN TRÁNSITO (Desde Carapungo hacia Llano Grande -> Lista para Confirmar Recepción en Sucursal)
            var req3 = new InternalRequisition
            {
                RequisitionCode = "REQ-2026-0003",
                RequisitionDate = DateTime.Now.AddDays(-1),
                IdRequestingWarehouse = branch1.IdWarehouse,
                IdFulfillingWarehouse = matrizWarehouse.IdWarehouse,
                IdUser = adminUser.IdUser,
                IdRequisitionType = 2, // Quiebre de stock crítico (SLA: 12 h)
                IdRequisitionStatus = 4, // DESPACHADO (En Tránsito)
                Observation = "Reposición por quiebre de stock en Llano Grande. Mercadería enviada en transporte interno.",
                ReviewedDate = DateTime.Now.AddHours(-5),
                IdReviewedByUser = adminUser.IdUser,
                ReviewNote = "Despachado con lotes FEFO desde Carapungo en guía interna #GI-0042."
            };
            req3.Details.Add(new InternalRequisitionDetail
            {
                IdProduct = p1.IdProduct,
                RequestedQuantity = 15,
                DispatchedQuantity = 15,
                Note = "Lote verificado FEFO"
            });
            req3.Details.Add(new InternalRequisitionDetail
            {
                IdProduct = p5.IdProduct,
                RequestedQuantity = 10,
                DispatchedQuantity = 10,
                Note = "Caja sellada"
            });

            // Requisición 4: APROBADO_PARCIAL (Campaña de vacunación/desparasitación en Pusuquí)
            var req4 = new InternalRequisition
            {
                RequisitionCode = "REQ-2026-0004",
                RequisitionDate = DateTime.Now.AddDays(-2),
                IdRequestingWarehouse = (branch2 ?? branch1).IdWarehouse,
                IdFulfillingWarehouse = matrizWarehouse.IdWarehouse,
                IdUser = adminUser.IdUser,
                IdRequisitionType = 4, // Pedido para campaña / jornada (SLA: 72 h)
                IdRequisitionStatus = 3, // APROBADO_PARCIAL
                Observation = "Insumos para jornada de atención veterinaria de fin de semana en Pusuquí.",
                ReviewedDate = DateTime.Now.AddDays(-1),
                IdReviewedByUser = adminUser.IdUser,
                ReviewNote = "Se aprueba entrega parcial de 15 unidades; el saldo se completará tras recepción de OC-2026-0002."
            };
            req4.Details.Add(new InternalRequisitionDetail
            {
                IdProduct = p4.IdProduct,
                RequestedQuantity = 30,
                DispatchedQuantity = 15,
                Note = "Entrega parcial autorizada"
            });

            // Requisición 5: COMPLETADO (Histórico ya recibido en sucursal Llano Grande)
            var req5 = new InternalRequisition
            {
                RequisitionCode = "REQ-2026-0005",
                RequisitionDate = DateTime.Now.AddDays(-4),
                IdRequestingWarehouse = branch1.IdWarehouse,
                IdFulfillingWarehouse = matrizWarehouse.IdWarehouse,
                IdUser = adminUser.IdUser,
                IdRequisitionType = 3, // Reposición ordinaria de stock (SLA: 48 h)
                IdRequisitionStatus = 5, // COMPLETADO
                Observation = "Pedido ordinario mensual recibido satisfactoriamente en sucursal.",
                ReviewedDate = DateTime.Now.AddDays(-3),
                IdReviewedByUser = adminUser.IdUser,
                ReviewNote = "Despachado desde Carapungo | Recepción: Recibido conforme en Llano Grande sin novedades."
            };
            req5.Details.Add(new InternalRequisitionDetail
            {
                IdProduct = p5.IdProduct,
                RequestedQuantity = 20,
                DispatchedQuantity = 20,
                Note = "Completado"
            });

            db.InternalRequisitions.AddRange(req1, req2, req3, req4, req5);
            await db.SaveChangesAsync();
        }

        // 9. Semilla de Movimientos de Inventario (Ingreso, Egreso, Traslado)
        if (!await db.InventoryMovements.AnyAsync() && matrizWarehouse != null)
        {
            var destBranch1 = branch1 ?? matrizWarehouse;
            var destBranch2 = branch2 ?? destBranch1;

            var entryType = await db.MovementTypes.FirstOrDefaultAsync(m => m.Mode == "Ingreso" || m.Code == "01" || m.Code == "ING" || m.Mode == "E")
                ?? await db.MovementTypes.FirstOrDefaultAsync();

            var saleType = await db.MovementTypes.FirstOrDefaultAsync(m => m.Mode == "Egreso" || m.Code == "02" || m.Code == "EGR" || m.Mode == "S")
                ?? entryType;

            var transferType = await db.MovementTypes.FirstOrDefaultAsync(m => m.Mode == "Traslado" || m.Code == "03" || m.Code == "TRA" || m.Mode == "T")
                ?? entryType;

            var prods = await db.Products.Where(p => p.IsActive).Take(10).ToListAsync();
            var lots = await db.Lots.Where(l => l.IsActive && l.Quantity > 0).ToListAsync();

            if (entryType != null && saleType != null && transferType != null && prods.Count >= 2)
            {
                var movsToSeed = new List<InventoryMovement>();

                // 1. Ingreso por compra a proveedor (CEDI Carapungo)
                var mov1 = new InventoryMovement
                {
                    MovementDate = DateTime.Now.AddDays(-15),
                    IdMovementType = entryType.IdMovementType,
                    IdDestinationWarehouse = matrizWarehouse.IdWarehouse,
                    IdOriginWarehouse = null,
                    IdUser = adminUser.IdUser,
                    Observation = "Ingreso de fármacos y biológicos por compra formal a proveedor Zoetis",
                    ReferenceSource = "ORDEN_COMPRA",
                    ReferenceId = 101
                };
                mov1.Details.Add(new InventoryMovementDetail
                {
                    IdProduct = prods[0].IdProduct,
                    IdLot = lots.FirstOrDefault(l => l.IdProduct == prods[0].IdProduct)?.IdLot ?? lots.FirstOrDefault()?.IdLot,
                    Quantity = 40,
                    UnitCost = prods[0].SuggestedPrice
                });
                if (prods.Count > 1)
                {
                    mov1.Details.Add(new InventoryMovementDetail
                    {
                        IdProduct = prods[1].IdProduct,
                        IdLot = lots.FirstOrDefault(l => l.IdProduct == prods[1].IdProduct)?.IdLot ?? lots.FirstOrDefault()?.IdLot,
                        Quantity = 30,
                        UnitCost = prods[1].SuggestedPrice
                    });
                }
                movsToSeed.Add(mov1);

                // 2. Ingreso para reposición periódica en CEDI Carapungo (Medvet Principal)
                var mov2 = new InventoryMovement
                {
                    MovementDate = DateTime.Now.AddDays(-10),
                    IdMovementType = entryType.IdMovementType,
                    IdDestinationWarehouse = matrizWarehouse.IdWarehouse,
                    IdOriginWarehouse = null,
                    IdUser = adminUser.IdUser,
                    Observation = "Ingreso de antibióticos y analgésicos veterinarios por orden de compra CEDI",
                    ReferenceSource = "REPOSICION_CENTRAL",
                    ReferenceId = 102
                };
                var p2 = prods.Count > 2 ? prods[2] : prods[0];
                mov2.Details.Add(new InventoryMovementDetail
                {
                    IdProduct = p2.IdProduct,
                    IdLot = lots.FirstOrDefault(l => l.IdProduct == p2.IdProduct)?.IdLot ?? lots.FirstOrDefault()?.IdLot,
                    Quantity = 25,
                    UnitCost = p2.SuggestedPrice
                });
                movsToSeed.Add(mov2);

                // 3. Egreso por venta clínica y farmacia (Matriz Carapungo)
                var mov3 = new InventoryMovement
                {
                    MovementDate = DateTime.Now.AddDays(-5),
                    IdMovementType = saleType.IdMovementType,
                    IdOriginWarehouse = matrizWarehouse.IdWarehouse,
                    IdDestinationWarehouse = null,
                    IdUser = adminUser.IdUser,
                    Observation = "Egreso por venta directa en mostrador de farmacia veterinaria",
                    ReferenceSource = "VENTA_FARMACIA",
                    ReferenceId = 201
                };
                mov3.Details.Add(new InventoryMovementDetail
                {
                    IdProduct = prods[0].IdProduct,
                    IdLot = lots.FirstOrDefault(l => l.IdProduct == prods[0].IdProduct)?.IdLot ?? lots.FirstOrDefault()?.IdLot,
                    Quantity = 5,
                    UnitCost = prods[0].SuggestedPrice
                });
                movsToSeed.Add(mov3);

                // 4. Egreso por atención médica de urgencias en Sucursal Llano Grande
                var mov4 = new InventoryMovement
                {
                    MovementDate = DateTime.Now.AddDays(-3),
                    IdMovementType = saleType.IdMovementType,
                    IdOriginWarehouse = destBranch1.IdWarehouse,
                    IdDestinationWarehouse = null,
                    IdUser = adminUser.IdUser,
                    Observation = "Egreso de antibióticos y analgésicos para cirugía canina",
                    ReferenceSource = "ATENCION_CLINICA",
                    ReferenceId = 202
                };
                var p4 = prods.Count > 3 ? prods[3] : prods[1];
                mov4.Details.Add(new InventoryMovementDetail
                {
                    IdProduct = p4.IdProduct,
                    IdLot = lots.FirstOrDefault(l => l.IdProduct == p4.IdProduct)?.IdLot ?? lots.FirstOrDefault()?.IdLot,
                    Quantity = 3,
                    UnitCost = p4.SuggestedPrice
                });
                movsToSeed.Add(mov4);

                // 5. Traslado desde Matriz Carapungo hacia Sucursal Llano Grande
                var mov5 = new InventoryMovement
                {
                    MovementDate = DateTime.Now.AddDays(-2),
                    IdMovementType = transferType.IdMovementType,
                    IdOriginWarehouse = matrizWarehouse.IdWarehouse,
                    IdDestinationWarehouse = destBranch1.IdWarehouse,
                    IdUser = adminUser.IdUser,
                    Observation = "Traslado planificado de mercadería desde CEDI Carapungo a Llano Grande",
                    ReferenceSource = "TRANSFERENCIA_INTERNA",
                    ReferenceId = 301
                };
                var p5 = prods.Count > 4 ? prods[4] : prods[0];
                mov5.Details.Add(new InventoryMovementDetail
                {
                    IdProduct = p5.IdProduct,
                    IdLot = lots.FirstOrDefault(l => l.IdProduct == p5.IdProduct)?.IdLot ?? lots.FirstOrDefault()?.IdLot,
                    Quantity = 10,
                    UnitCost = p5.SuggestedPrice
                });
                movsToSeed.Add(mov5);

                // 6. Traslado desde Matriz Carapungo hacia Sucursal Pusuquí
                var mov6 = new InventoryMovement
                {
                    MovementDate = DateTime.Now.AddHours(-6),
                    IdMovementType = transferType.IdMovementType,
                    IdOriginWarehouse = matrizWarehouse.IdWarehouse,
                    IdDestinationWarehouse = destBranch2.IdWarehouse,
                    IdUser = adminUser.IdUser,
                    Observation = "Traslado de urgencia para abastecimiento de Sucursal Pusuquí",
                    ReferenceSource = "REQUISICION_INTERNA",
                    ReferenceId = 302
                };
                var p6 = prods.Count > 5 ? prods[5] : prods[1];
                mov6.Details.Add(new InventoryMovementDetail
                {
                    IdProduct = p6.IdProduct,
                    IdLot = lots.FirstOrDefault(l => l.IdProduct == p6.IdProduct)?.IdLot ?? lots.FirstOrDefault()?.IdLot,
                    Quantity = 8,
                    UnitCost = p6.SuggestedPrice
                });
                movsToSeed.Add(mov6);

                db.InventoryMovements.AddRange(movsToSeed);
                await db.SaveChangesAsync();
            }
        }

        // Regla estricta: Garantizar que todo Ingreso histórico o existente apunte exclusivamente a la Bodega Matriz
        if (matrizWarehouse != null)
        {
            var branchIngresos = await db.InventoryMovements
                .Include(m => m.MovementType)
                .Where(m => m.MovementType != null && 
                            (m.MovementType.Mode == "Ingreso" || m.MovementType.Mode == "E") && 
                            m.IdDestinationWarehouse != matrizWarehouse.IdWarehouse)
                .ToListAsync();

            if (branchIngresos.Count > 0)
            {
                foreach (var m in branchIngresos)
                {
                    m.IdDestinationWarehouse = matrizWarehouse.IdWarehouse;
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
