using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace NotificationService.Repositories.Interfaces
{
    public interface INotificationGroupRepository : IBaseEFRepository<NotificationGroup, string>
    {
        Task<NotificationGroup?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<NotificationGroup?> GetByIdWithMembersAsync(string id, CancellationToken cancellationToken = default);
        Task<List<NotificationGroup>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    }
}