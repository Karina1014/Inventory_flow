using System;
using Inventory_Flow.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Data;

public class AplicationDbContext: DbContext
{
    public  AplicationDbContext (DbContextOptions<AplicationDbContext> options): base(options)
    {
        
    } 
    public DbSet <User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Warehouse> Warehouses {get;set;}
    public DbSet<Category> Categories {get;set;}
    public DbSet<Product> Products {get;set;}
    public DbSet<Lot> Lots {get;set;}
    public DbSet<MovementType> MovementTypes {get;set;}
    public DbSet<InventoryMovement> InventoryMovements {get;set;}
    public DbSet<InventoryMovementDetail> InventoryMovementDetails {get;set;}
    public DbSet<ThresholdInvetory> ThresholdInvetories { get; set; }
    public DbSet<Menu>Menus {get;set;}
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<InternalRequisition> InternalRequisitions { get; set; }
    public DbSet<InternalRequisitionDetail> InternalRequisitionDetails { get; set; }
    public DbSet<RequisitionStatus> RequisitionStatuses { get; set; }
    public DbSet<RequisitionType> RequisitionTypes { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
    public DbSet<PurchaseOrderStatus> PurchaseOrderStatuses { get; set; }
    public DbSet<PurchaseOrderType> PurchaseOrderTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Catálogos de requisición ----------
        modelBuilder.Entity<RequisitionStatus>()
            .HasIndex(s => s.Code)
            .IsUnique();

        modelBuilder.Entity<RequisitionType>()
            .HasIndex(t => t.Code)
            .IsUnique();

        modelBuilder.Entity<InternalRequisition>()
            .HasIndex(r => r.RequisitionCode)
            .IsUnique();

        modelBuilder.Entity<InternalRequisition>()
            .HasOne(r => r.RequisitionStatus)
            .WithMany()
            .HasForeignKey(r => r.IdRequisitionStatus)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternalRequisition>()
            .HasOne(r => r.RequisitionType)
            .WithMany()
            .HasForeignKey(r => r.IdRequisitionType)
            .OnDelete(DeleteBehavior.Restrict);

        // Dos FK hacia User: se restringen para evitar múltiples rutas de cascada en SQL Server
        modelBuilder.Entity<InternalRequisition>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.IdUser)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternalRequisition>()
            .HasOne(r => r.ReviewedByUser)
            .WithMany()
            .HasForeignKey(r => r.IdReviewedByUser)
            .OnDelete(DeleteBehavior.Restrict);

        // Datos semilla (IDs fijos para que la migración sea determinista)
        modelBuilder.Entity<RequisitionStatus>().HasData(
            new RequisitionStatus { IdRequisitionStatus = 1, Code = "SOLICITADO",        Name = "Solicitado",       Description = "Enviada por la sucursal, pendiente de revisión en Carapungo.", BadgeColor = "warning",   SortOrder = 1, IsFinal = false },
            new RequisitionStatus { IdRequisitionStatus = 2, Code = "EN_REVISION",       Name = "En revisión",      Description = "Carapungo está verificando disponibilidad de stock.",          BadgeColor = "info",      SortOrder = 2, IsFinal = false },
            new RequisitionStatus { IdRequisitionStatus = 3, Code = "APROBADO_PARCIAL",  Name = "Aprobado parcial", Description = "Se atiende solo una parte; el faltante pasa a compras.",        BadgeColor = "primary",   SortOrder = 3, IsFinal = false },
            new RequisitionStatus { IdRequisitionStatus = 4, Code = "DESPACHADO",         Name = "Despachado",       Description = "La mercadería salió de Carapungo hacia la sucursal.",          BadgeColor = "primary",   SortOrder = 4, IsFinal = false },
            new RequisitionStatus { IdRequisitionStatus = 5, Code = "COMPLETADO",         Name = "Completado",       Description = "La sucursal confirmó la recepción.",                          BadgeColor = "success",   SortOrder = 5, IsFinal = true  },
            new RequisitionStatus { IdRequisitionStatus = 6, Code = "RECHAZADO",          Name = "Rechazado",        Description = "Carapungo rechazó la solicitud.",                             BadgeColor = "danger",    SortOrder = 6, IsFinal = true  },
            new RequisitionStatus { IdRequisitionStatus = 7, Code = "CANCELADO",          Name = "Cancelado",        Description = "La solicitud fue anulada.",                                   BadgeColor = "secondary", SortOrder = 7, IsFinal = true  }
        );

        modelBuilder.Entity<RequisitionType>().HasData(
            new RequisitionType { IdRequisitionType = 1, Code = "URGENCIA_QUIRURGICA", Name = "Urgencia quirúrgica / emergencia", Description = "Insumos críticos para cirugías o emergencias médicas.", PriorityLevel = 1, MaxResponseHours = 4,  IsActive = true },
            new RequisitionType { IdRequisitionType = 2, Code = "PACIENTE_ESPECIFICO", Name = "Paciente específico",              Description = "Medicamento bajo pedido para un tratamiento puntual.",  PriorityLevel = 2, MaxResponseHours = 24, IsActive = true },
            new RequisitionType { IdRequisitionType = 3, Code = "CAMPANIA_SANITARIA",  Name = "Campaña sanitaria / vacunación",   Description = "Stock adicional para jornadas planificadas.",           PriorityLevel = 3, MaxResponseHours = 48, IsActive = true },
            new RequisitionType { IdRequisitionType = 4, Code = "REPOSICION_ORDINARIA", Name = "Reposición ordinaria",            Description = "Reabastecimiento rutinario semanal de la sucursal.",    PriorityLevel = 4, MaxResponseHours = 72, IsActive = true }
        );

        // Requisiciones internas: cabecera -> detalle en cascada
        modelBuilder.Entity<InternalRequisition>()
            .HasMany(r => r.Details)
            .WithOne(d => d.InternalRequisition)
            .HasForeignKey(d => d.IdInternalRequisition)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InternalRequisition>()
            .HasOne(r => r.RequestingWarehouse)
            .WithMany()
            .HasForeignKey(r => r.IdRequestingWarehouse)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternalRequisition>()
            .HasOne(r => r.FulfillingWarehouse)
            .WithMany()
            .HasForeignKey(r => r.IdFulfillingWarehouse)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternalRequisitionDetail>()
            .HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);

        // Cabecera -> detalle: el detalle solo existe con su movimiento.
        modelBuilder.Entity<InventoryMovement>()
            .HasMany(m => m.Details)
            .WithOne(d => d.InventoryMovement)
            .HasForeignKey(d => d.IdInventoryMovement)
            .OnDelete(DeleteBehavior.Cascade);

        // No se permite borrar un tipo de movimiento que tenga movimientos.
        modelBuilder.Entity<InventoryMovement>()
            .HasOne(m => m.MovementType)
            .WithMany()
            .HasForeignKey(m => m.IdMovementType)
            .OnDelete(DeleteBehavior.Restrict);

        // No se permite borrar un producto que tenga movimientos registrados.
        modelBuilder.Entity<InventoryMovementDetail>()
            .HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación User -> Warehouse (Restringir borrado para evitar inconsistencias)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Warehouse)
            .WithMany()
            .HasForeignKey(u => u.IdWarehouse)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Menu -> Role
        modelBuilder.Entity<Menu>()
            .HasOne(m => m.Role)
            .WithMany()
            .HasForeignKey(m => m.IdRole)
            .OnDelete(DeleteBehavior.Cascade);

        // ---------- Órdenes de Compra y Aprobación Financiera ----------
        modelBuilder.Entity<PurchaseOrderStatus>()
            .HasIndex(s => s.Code)
            .IsUnique();

        modelBuilder.Entity<PurchaseOrderType>()
            .HasIndex(t => t.Code)
            .IsUnique();

        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        // Cabecera -> detalle en cascada
        modelBuilder.Entity<PurchaseOrder>()
            .HasMany(o => o.Details)
            .WithOne(d => d.PurchaseOrder)
            .HasForeignKey(d => d.IdPurchaseOrder)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(o => o.Supplier)
            .WithMany()
            .HasForeignKey(o => o.IdSupplier)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(o => o.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(o => o.IdDestinationWarehouse)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(o => o.PurchaseOrderType)
            .WithMany()
            .HasForeignKey(o => o.IdPurchaseOrderType)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(o => o.PurchaseOrderStatus)
            .WithMany()
            .HasForeignKey(o => o.IdPurchaseOrderStatus)
            .OnDelete(DeleteBehavior.Restrict);

        // Claves foráneas hacia User con Restrict para evitar ciclos de cascada
        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.IdUser)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(o => o.FinancialApprovalUser)
            .WithMany()
            .HasForeignKey(o => o.IdFinancialApprovalUser)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrderDetail>()
            .HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrderDetail>()
            .HasOne(d => d.InternalRequisition)
            .WithMany()
            .HasForeignKey(d => d.IdInternalRequisition)
            .OnDelete(DeleteBehavior.SetNull);

        // Catálogos Semilla con IDs deterministas (Cero datos quemados)
        modelBuilder.Entity<PurchaseOrderStatus>().HasData(
            new PurchaseOrderStatus { IdPurchaseOrderStatus = 1, Code = "BORRADOR",             Name = "Borrador",                      Description = "En preparación por compras / bodega central.",                BadgeColor = "secondary", SortOrder = 1, IsFinal = false },
            new PurchaseOrderStatus { IdPurchaseOrderStatus = 2, Code = "PENDIENTE_FINANCIERO", Name = "Pendiente Aprobación Financiera", Description = "Enviada a dirección financiera para validación de gasto.",     BadgeColor = "warning",   SortOrder = 2, IsFinal = false },
            new PurchaseOrderStatus { IdPurchaseOrderStatus = 3, Code = "APROBADO_FINANCIERO",  Name = "Aprobado por Finanzas",         Description = "Presupuesto aprobado y lista para emitir a proveedor.",       BadgeColor = "info",      SortOrder = 3, IsFinal = false },
            new PurchaseOrderStatus { IdPurchaseOrderStatus = 4, Code = "RECHAZADO_FINANCIERO", Name = "Rechazado por Finanzas",        Description = "Rechazado o devuelto con observaciones financieras.",         BadgeColor = "danger",    SortOrder = 4, IsFinal = false },
            new PurchaseOrderStatus { IdPurchaseOrderStatus = 5, Code = "EMITIDO_PROVEEDOR",   Name = "Emitido al Proveedor",          Description = "Orden enviada formalmente al proveedor para despacho.",        BadgeColor = "primary",   SortOrder = 5, IsFinal = false },
            new PurchaseOrderStatus { IdPurchaseOrderStatus = 6, Code = "RECIBIDO_PARCIAL",    Name = "Recibido Parcial",              Description = "Mercadería ingresada parcialmente en Carapungo.",             BadgeColor = "primary",   SortOrder = 6, IsFinal = false },
            new PurchaseOrderStatus { IdPurchaseOrderStatus = 7, Code = "RECIBIDO_TOTAL",      Name = "Recibido Completo",             Description = "Total de ítems recibidos e ingresados al inventario.",        BadgeColor = "success",   SortOrder = 7, IsFinal = true  },
            new PurchaseOrderStatus { IdPurchaseOrderStatus = 8, Code = "CANCELADO",           Name = "Cancelado",                     Description = "Orden de compra anulada formalmente.",                        BadgeColor = "secondary", SortOrder = 8, IsFinal = true  }
        );

        modelBuilder.Entity<PurchaseOrderType>().HasData(
            new PurchaseOrderType { IdPurchaseOrderType = 1, Code = "DESABASTECIMIENTO_CRITICO", Name = "Desabastecimiento Crítico",  Description = "Compra urgente de fármacos o insumos esenciales agotados.", PriorityLevel = 1, IsActive = true },
            new PurchaseOrderType { IdPurchaseOrderType = 2, Code = "REPOSICION_CENTRAL",        Name = "Reposición Periódica CEDI",  Description = "Reabastecimiento regular de stock de seguridad para Carapungo.", PriorityLevel = 2, IsActive = true },
            new PurchaseOrderType { IdPurchaseOrderType = 3, Code = "CAMPANIA_ESTACIONAL",       Name = "Campaña / Temporada",        Description = "Volumen extraordinario para campañas masivas de vacunación.", PriorityLevel = 3, IsActive = true },
            new PurchaseOrderType { IdPurchaseOrderType = 4, Code = "PEDIDO_ESPECIAL_IMPORTADO", Name = "Bajo Pedido Especial",       Description = "Insumos o equipos específicos no habituales en inventario.", PriorityLevel = 4, IsActive = true }
        );
    }
}
