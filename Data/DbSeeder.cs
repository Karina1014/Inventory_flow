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

        // Menús para BODEGUERO PRINCIPAL (Gestión de CEDI Carapungo y compras)
        if (bodegueroPrincipalRole != null)
        {
            await EnsureMenuAsync("Dashboard", "", "/dashboard", "bi bi-grid-fill", 1, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Control de Inventario", "Operaciones", "/inventario", "bi bi-box-seam", 2, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Requisiciones", "Operaciones", "/requisitions", "bi bi-send-check", 3, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Órdenes de Compra", "Operaciones", "/purchase-orders", "bi bi-cart-check", 4, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Lotes", "Operaciones", "/lots", "bi bi-boxes", 5, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Movimientos", "Operaciones", "/movements", "bi bi-arrow-left-right", 6, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Productos", "Catálogos", "/products", "bi bi-box-seam", 7, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Proveedores", "Catálogos", "/suppliers", "bi bi-truck", 8, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Reporte de stock", "Reportes", "/stockreport", "bi bi-clipboard-data", 9, bodegueroPrincipalRole.IdRole);
            await EnsureMenuAsync("Kardex", "Reportes", "/kardex", "bi bi-journal-text", 10, bodegueroPrincipalRole.IdRole);
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

        // Menús para FINANCIERO (Aprobación de compras y auditoría de Kardex)
        if (financieroRole != null)
        {
            await EnsureMenuAsync("Dashboard", "", "/dashboard", "bi bi-grid-fill", 1, financieroRole.IdRole);
            await EnsureMenuAsync("Aprobación de Compras", "Finanzas", "/purchase-orders", "bi bi-cash-coin", 2, financieroRole.IdRole);
            await EnsureMenuAsync("Kardex", "Finanzas", "/kardex", "bi bi-journal-text", 3, financieroRole.IdRole);
            await EnsureMenuAsync("Reporte de stock", "Reportes", "/stockreport", "bi bi-clipboard-data", 4, financieroRole.IdRole);
            await EnsureMenuAsync("Análisis IA", "Reportes", "/analytics", "bi bi-stars", 5, financieroRole.IdRole);
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

        // 5. Semilla de Órdenes de Compra Demostrativas si la tabla está vacía
        if (!await db.PurchaseOrders.AnyAsync())
        {
            var firstSupplier = await db.Suppliers.FirstOrDefaultAsync();
            var secondSupplier = await db.Suppliers.Skip(1).FirstOrDefaultAsync() ?? firstSupplier;
            var matrizWarehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Name.Contains("Carapungo") || w.Name.Contains("Matriz"))
                ?? await db.Warehouses.FirstOrDefaultAsync();
            var adminUser = await db.Users.FirstOrDefaultAsync() ?? new User { IdUser = 1 };
            var sampleProduct = await db.Products.FirstOrDefaultAsync();

            if (firstSupplier != null && matrizWarehouse != null)
            {
                // Orden 1: Pendiente de Aprobación Financiera (Se verá de inmediato en la pestaña de Finanzas con badge)
                var order1 = new PurchaseOrder
                {
                    OrderNumber = "OC-2026-0001",
                    OrderDate = DateTime.Now.AddHours(-3),
                    IdSupplier = firstSupplier.IdSupplier,
                    IdDestinationWarehouse = matrizWarehouse.IdWarehouse,
                    IdUser = adminUser.IdUser,
                    IdPurchaseOrderType = 1, // Desabastecimiento Crítico
                    IdPurchaseOrderStatus = 2, // PENDIENTE_FINANCIERO
                    ExpectedDeliveryDate = DateTime.Now.AddDays(2),
                    Observation = "Reabastecimiento urgente de fármacos antibióticos y sedantes para quirófano central Carapungo.",
                    Subtotal = 450.00m,
                    TaxAmount = 67.50m,
                    TotalAmount = 517.50m
                };

                if (sampleProduct != null)
                {
                    order1.Details.Add(new PurchaseOrderDetail
                    {
                        IdProduct = sampleProduct.IdProduct,
                        Quantity = 30,
                        ReceivedQuantity = 0,
                        UnitPrice = 15.00m,
                        Subtotal = 450.00m,
                        TaxPercentage = 15.00m,
                        TaxAmount = 67.50m,
                        Total = 517.50m,
                        Notes = "Lote con vencimiento no menor a 18 meses"
                    });
                }

                // Orden 2: Aprobada por Finanzas
                var order2 = new PurchaseOrder
                {
                    OrderNumber = "OC-2026-0002",
                    OrderDate = DateTime.Now.AddDays(-2),
                    IdSupplier = secondSupplier?.IdSupplier ?? firstSupplier.IdSupplier,
                    IdDestinationWarehouse = matrizWarehouse.IdWarehouse,
                    IdUser = adminUser.IdUser,
                    IdPurchaseOrderType = 2, // Reposición Periódica CEDI
                    IdPurchaseOrderStatus = 3, // APROBADO_FINANCIERO
                    ExpectedDeliveryDate = DateTime.Now.AddDays(3),
                    Observation = "Reposición mensual planificada de vacunas y desparasitantes.",
                    Subtotal = 1200.00m,
                    TaxAmount = 180.00m,
                    TotalAmount = 1380.00m,
                    FinancialApprovalDate = DateTime.Now.AddDays(-1),
                    IdFinancialApprovalUser = adminUser.IdUser,
                    FinancialApprovalNote = "Aprobado por Dirección Financiera conforme al presupuesto operativo del mes."
                };

                if (sampleProduct != null)
                {
                    order2.Details.Add(new PurchaseOrderDetail
                    {
                        IdProduct = sampleProduct.IdProduct,
                        Quantity = 80,
                        ReceivedQuantity = 0,
                        UnitPrice = 15.00m,
                        Subtotal = 1200.00m,
                        TaxPercentage = 15.00m,
                        TaxAmount = 180.00m,
                        Total = 1380.00m,
                        Notes = "Mantener cadena de frío 2-8°C durante transporte"
                    });
                }

                // Orden 3: Borrador
                var order3 = new PurchaseOrder
                {
                    OrderNumber = "OC-2026-0003",
                    OrderDate = DateTime.Now.AddMinutes(-30),
                    IdSupplier = firstSupplier.IdSupplier,
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

                db.PurchaseOrders.AddRange(order1, order2, order3);
                await db.SaveChangesAsync();
            }
        }

        // 6. Semilla de Requisiciones Internas si la tabla está vacía
        if (!await db.InternalRequisitions.AnyAsync())
        {
            var matrizWarehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Name.Contains("Carapungo") || w.Name.Contains("Matriz"));
            var branchWarehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.IdWarehouse != (matrizWarehouse != null ? matrizWarehouse.IdWarehouse : 0))
                ?? matrizWarehouse;
            var adminUser = await db.Users.FirstOrDefaultAsync() ?? new User { IdUser = 1 };
            var sampleProduct = await db.Products.FirstOrDefaultAsync();

            if (branchWarehouse != null && matrizWarehouse != null)
            {
                var req1 = new InternalRequisition
                {
                    RequisitionCode = "REQ-2026-0001",
                    RequisitionDate = DateTime.Now.AddHours(-5),
                    IdRequestingWarehouse = branchWarehouse.IdWarehouse,
                    IdFulfillingWarehouse = matrizWarehouse.IdWarehouse,
                    IdUser = adminUser.IdUser,
                    IdRequisitionType = 1, // Urgencia Quirúrgica
                    IdRequisitionStatus = 1, // SOLICITADO
                    Observation = "Insumos urgentes para cirugía programada en sucursal."
                };

                if (sampleProduct != null)
                {
                    req1.Details.Add(new InternalRequisitionDetail
                    {
                        IdProduct = sampleProduct.IdProduct,
                        RequestedQuantity = 10,
                        DispatchedQuantity = 0,
                        Note = "Urgente"
                    });
                }

                db.InternalRequisitions.Add(req1);
                await db.SaveChangesAsync();
            }
        }
    }
}
