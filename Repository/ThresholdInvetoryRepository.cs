using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class ThresholdInvetoryRepository : IThresholdInvetoryRepository
{
    private readonly AplicationDbContext _db;

    public ThresholdInvetoryRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<ThresholdInvetory>> GetThresholdsAsync()
    {
        return await _db.ThresholdInvetories.OrderByDescending(t => t.IdThresholdInvetory).ToListAsync();
    }

    public async Task<ThresholdInvetory?> GetThresholdByIdAsync(int id)
    {
        return await _db.ThresholdInvetories.FirstOrDefaultAsync(t => t.IdThresholdInvetory == id);
    }

    public async Task<ThresholdInvetory?> GetCurrentThresholdAsync()
    {
        return await _db.ThresholdInvetories
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.UpdatedAt)
            .ThenByDescending(t => t.IdThresholdInvetory)
            .FirstOrDefaultAsync();
    }

    public async Task<ThresholdInvetory> CreateThresholdAsync(CreateThresholdInvetoryDto dto)
    {
        // Si el nuevo se crea activo, desactivamos los anteriores para que solo haya uno vigente
        if (dto.IsActive)
        {
            var activeThresholds = await _db.ThresholdInvetories.Where(t => t.IsActive).ToListAsync();
            foreach (var item in activeThresholds)
            {
                item.IsActive = false;
            }
        }

        var threshold = new ThresholdInvetory
        {
            Threshold = dto.Threshold,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.ThresholdInvetories.Add(threshold);
        await _db.SaveChangesAsync();
        return threshold;
    }

    public async Task<ThresholdInvetory> UpdateThresholdAsync(int id, UpdateThresholdInvetoryDto dto)
    {
        var threshold = await _db.ThresholdInvetories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Umbral con Id {id} no encontrado.");

        // Si se activa este umbral, desactivamos los otros para mantener integridad
        if (dto.IsActive && !threshold.IsActive)
        {
            var activeThresholds = await _db.ThresholdInvetories.Where(t => t.IsActive && t.IdThresholdInvetory != id).ToListAsync();
            foreach (var item in activeThresholds)
            {
                item.IsActive = false;
            }
        }

        threshold.Threshold = dto.Threshold;
        threshold.IsActive = dto.IsActive;
        threshold.UpdatedAt = DateTime.Now;

        _db.ThresholdInvetories.Update(threshold);
        await _db.SaveChangesAsync();
        return threshold;
    }

    public async Task<bool> DeleteThresholdAsync(int id)
    {
        var threshold = await _db.ThresholdInvetories.FindAsync(id);
        if (threshold == null) return false;

        _db.ThresholdInvetories.Remove(threshold);
        await _db.SaveChangesAsync();
        return true;
    }
}
