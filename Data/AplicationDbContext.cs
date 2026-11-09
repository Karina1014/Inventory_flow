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
}
