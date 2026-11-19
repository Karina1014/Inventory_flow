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

implementar despacho fefo en transito, confirmacion en sucursal y seeder completo        // 5. Identificar Bodega Matriz (Carapungo) y Sucursales Periféricas
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
    }
}
