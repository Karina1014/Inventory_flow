using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;

namespace Inventory_Flow.Repository.IRepository;

public interface IN8nNotificationRepository
{
    Task<N8nNotification> CreateAsync(N8nNotification notification);
    Task<bool> UpdateStatusAsync(int idN8nNotification, string status, string response);
    Task<ICollection<N8nNotification>> GetAllAsync();
    Task<ICollection<N8nNotification>> GetRecentAsync(int count = 20);
    Task<N8nNotification?> GetByIdAsync(int idN8nNotification);
    Task<ICollection<N8nNotificationType>> GetNotificationTypesAsync();
}
