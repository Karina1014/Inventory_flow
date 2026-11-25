using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class N8nNotificationRepository : IN8nNotificationRepository
{
    private readonly AplicationDbContext _db;

    public N8nNotificationRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public async Task<N8nNotification> CreateAsync(N8nNotification notification)
    {
        await _db.N8nNotifications.AddAsync(notification);
        await _db.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> UpdateStatusAsync(int idN8nNotification, string status, string response)
    {
        var entity = await _db.N8nNotifications.FindAsync(idN8nNotification);
        if (entity == null) return false;

        entity.Status = status;
        entity.Response = response ?? string.Empty;
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<ICollection<N8nNotification>> GetAllAsync()
    {
        return await _db.N8nNotifications
            .Include(n => n.NotificationType)
            .Include(n => n.Warehouse)
            .Include(n => n.User)
            .OrderByDescending(n => n.SentDate)
            .ToListAsync();
    }

    public async Task<ICollection<N8nNotification>> GetRecentAsync(int count = 20)
    {
        return await _db.N8nNotifications
            .Include(n => n.NotificationType)
            .Include(n => n.Warehouse)
            .Include(n => n.User)
            .OrderByDescending(n => n.SentDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<N8nNotification?> GetByIdAsync(int idN8nNotification)
    {
        return await _db.N8nNotifications
            .Include(n => n.NotificationType)
            .Include(n => n.Warehouse)
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.IdN8nNotification == idN8nNotification);
    }

    public async Task<ICollection<N8nNotificationType>> GetNotificationTypesAsync()
    {
        return await _db.N8nNotificationTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.IdN8nNotificationType)
            .ToListAsync();
    }
}
