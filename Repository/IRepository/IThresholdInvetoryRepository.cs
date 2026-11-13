using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IThresholdInvetoryRepository
{
    Task<ICollection<ThresholdInvetory>> GetThresholdsAsync();
    Task<ThresholdInvetory?> GetThresholdByIdAsync(int id);
    Task<ThresholdInvetory?> GetCurrentThresholdAsync();
    Task<ThresholdInvetory> CreateThresholdAsync(CreateThresholdInvetoryDto dto);
    Task<ThresholdInvetory> UpdateThresholdAsync(int id, UpdateThresholdInvetoryDto dto);
    Task<bool> DeleteThresholdAsync(int id);
}

