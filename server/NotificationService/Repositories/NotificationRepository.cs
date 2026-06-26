using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.Repositories
{
    public class NotificationRepository : BaseEFRepository<Notification, string, NotificationSvcDbContext>, INotificationRepository
    {
        public NotificationRepository(NotificationSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Notification>> GetByUserIdWithRecipientsAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(n => n.Recipients)
                .Where(n => n.Recipients.Any(r => r.UserId == userId))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetByIdsAndUserIdWithRecipientsAsync(IList<string> notificationIds, string userId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(n => n.Recipients)
                .Where(n => notificationIds.Contains(n.Id) && n.Recipients.Any(r => r.UserId == userId))
                .ToListAsync(cancellationToken);
        }
    }
}
