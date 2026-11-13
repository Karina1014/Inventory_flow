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
        return await _db.ThresholdInvetories.OrderByDescending(t => t.IdThresholdInvetory).FirstOrDefaultAsync();
    }

    public async Task<ThresholdInvetory> CreateThresholdAsync(CreateThresholdInvetoryDto dto)
    {
        var threshold = new ThresholdInvetory
        {
            Threshold = dto.Threshold,
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

        threshold.Threshold = dto.Threshold;
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

