using NotificationService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace NotificationService.Repositories.Interfaces
{
    public interface INotificationRepository : IBaseEFRepository<Notification, string>
    {
        Task<List<Notification>> GetByUserIdWithRecipientsAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<Notification>> GetByIdsAndUserIdWithRecipientsAsync(IList<string> notificationIds, string userId, CancellationToken cancellationToken = default);
    }
}
