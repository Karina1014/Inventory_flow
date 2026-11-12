using System.Threading.Tasks;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IDashboardRepository
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync();
}

