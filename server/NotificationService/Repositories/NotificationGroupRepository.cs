using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.Repositories
{
    public class NotificationGroupRepository : BaseEFRepository<NotificationGroup, string, NotificationSvcDbContext>, INotificationGroupRepository
    {
        public NotificationGroupRepository(NotificationSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<NotificationGroup?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await DbSet.FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
        }

        public async Task<NotificationGroup?> GetByIdWithMembersAsync(string id, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        }

        public async Task<List<NotificationGroup>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(ng => ng.Members.Any(m => m.UserId == userId))
                .ToListAsync(cancellationToken);
        }
    }
}
