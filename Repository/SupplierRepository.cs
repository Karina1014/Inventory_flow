using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Extensions;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class SupplierRepository : ISupplierRepository
{
    private readonly AplicationDbContext _db;

    public SupplierRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public ICollection<Supplier> GetSuppliers()
    {
        return _db.Suppliers.OrderBy(s => s.CompanyName).ToList();
    }

    public async Task<PagedResultDto<Supplier>> GetSuppliersPagedAsync(int pageNumber, int pageSize, string? search = null, bool? activeOnly = null)
    {
        var query = _db.Suppliers.AsQueryable();

        if (activeOnly.HasValue && activeOnly.Value)
        {
            query = query.Where(s => s.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                (s.CompanyName != null && s.CompanyName.ToLower().Contains(term)) ||
                (s.Ruc != null && s.Ruc.Contains(term)) ||
                (s.ContactName != null && s.ContactName.ToLower().Contains(term)) ||
                (s.Email != null && s.Email.ToLower().Contains(term)));
        }

        return await query
            .OrderByDescending(s => s.CreationDate)
            .ThenByDescending(s => s.IdSupplier)
            .ToPagedResultAsync(pageNumber, pageSize);
    }

    public Supplier? GetSupplier(int idSupplier)
    {
        return _db.Suppliers.FirstOrDefault(s => s.IdSupplier == idSupplier);
    }

    public bool IsUniqueRuc(string ruc, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(ruc)) return false;
        var cleanRuc = ruc.Trim();

        var query = _db.Suppliers.AsQueryable();
        if (excludeId.HasValue && excludeId.Value > 0)
        {
            query = query.Where(s => s.IdSupplier != excludeId.Value);
        }

        return !query.Any(s => s.Ruc == cleanRuc);
    }

    public async Task<Supplier> CreateSupplier(CreateSupplierDto dto)
    {
        var supplier = new Supplier
        {
            Ruc = dto.Ruc.Trim(),
            CompanyName = dto.CompanyName.Trim(),
            ContactName = dto.ContactName?.Trim() ?? string.Empty,
            Phone = dto.Phone?.Trim() ?? string.Empty,
            Email = dto.Email?.Trim() ?? string.Empty,
            Address = dto.Address?.Trim() ?? string.Empty,
            DeliveryLeadTimeDays = dto.DeliveryLeadTimeDays,
            IsActive = dto.IsActive,
            CreationDate = DateTime.Now
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier> UpdateSupplier(int idSupplier, UpdateSupplierDto dto)
    {
        var supplier = await _db.Suppliers.FindAsync(idSupplier)
            ?? throw new KeyNotFoundException($"Proveedor con Id {idSupplier} no encontrado.");

        supplier.Ruc = dto.Ruc.Trim();
        supplier.CompanyName = dto.CompanyName.Trim();
        supplier.ContactName = dto.ContactName?.Trim() ?? string.Empty;
        supplier.Phone = dto.Phone?.Trim() ?? string.Empty;
        supplier.Email = dto.Email?.Trim() ?? string.Empty;
        supplier.Address = dto.Address?.Trim() ?? string.Empty;
        supplier.DeliveryLeadTimeDays = dto.DeliveryLeadTimeDays;
        supplier.IsActive = dto.IsActive;

        _db.Suppliers.Update(supplier);
        await _db.SaveChangesAsync();
        return supplier;
    }

    public bool DeleteSupplier(int idSupplier)
    {
        var supplier = _db.Suppliers.FirstOrDefault(s => s.IdSupplier == idSupplier);
        if (supplier == null) return false;

        _db.Suppliers.Remove(supplier);
        _db.SaveChanges();
        return true;
    }
}
