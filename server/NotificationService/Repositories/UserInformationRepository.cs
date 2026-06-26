using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.Repositories
{
    public class UserInformationRepository : BaseEFRepository<UserInformation, int, NotificationSvcDbContext>, IUserInformationRepository
    {
        public UserInformationRepository(NotificationSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UserInformation>> GetByUserIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(u => userIds.Contains(u.UserId)).ToListAsync(cancellationToken);
        }
    }
}