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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }
}
