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
            new RequisitionStatus { IdRequisitionStatus = 1, Code = RequisitionStatusCodes.Solicitado,      Name = "Solicitado",       Description = "Enviada por la sucursal, pendiente de revisión en Carapungo.", BadgeColor = "warning",   SortOrder = 1, IsFinal = false },
            new RequisitionStatus { IdRequisitionStatus = 2, Code = RequisitionStatusCodes.EnRevision,      Name = "En revisión",      Description = "Carapungo está verificando disponibilidad de stock.",          BadgeColor = "info",      SortOrder = 2, IsFinal = false },
            new RequisitionStatus { IdRequisitionStatus = 3, Code = RequisitionStatusCodes.AprobadoParcial, Name = "Aprobado parcial", Description = "Se atiende solo una parte; el faltante pasa a compras.",        BadgeColor = "primary",   SortOrder = 3, IsFinal = false },
            new RequisitionStatus { IdRequisitionStatus = 4, Code = RequisitionStatusCodes.Despachado,      Name = "Despachado",       Description = "La mercadería salió de Carapungo hacia la sucursal.",          BadgeColor = "primary",   SortOrder = 4, IsFinal = false },
            new RequisitionStatus { IdRequisitionStatus = 5, Code = RequisitionStatusCodes.Completado,      Name = "Completado",       Description = "La sucursal confirmó la recepción.",                          BadgeColor = "success",   SortOrder = 5, IsFinal = true  },
            new RequisitionStatus { IdRequisitionStatus = 6, Code = RequisitionStatusCodes.Rechazado,       Name = "Rechazado",        Description = "Carapungo rechazó la solicitud.",                             BadgeColor = "danger",    SortOrder = 6, IsFinal = true  },
            new RequisitionStatus { IdRequisitionStatus = 7, Code = RequisitionStatusCodes.Cancelado,       Name = "Cancelado",        Description = "La solicitud fue anulada.",                                   BadgeColor = "secondary", SortOrder = 7, IsFinal = true  }
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
    }
}
