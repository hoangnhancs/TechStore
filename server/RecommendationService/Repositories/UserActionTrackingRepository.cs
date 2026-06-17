using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using RecommendationService.Data;
using RecommendationService.Entities;
using RecommendationService.Repositories.Interface;

namespace RecommendationService.Repositories
{
    public class UserActionTrackingRepository : BaseEFRepository<UserActionTracking, int, RecommandationSvcDbContext>, IUserActionTrackingRepository
    {
        public UserActionTrackingRepository(RecommandationSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UserActionTracking>> GetUserActionTrackingByUserId(string userId, CancellationToken cancellationToken)
        {
            return await _dbContext.UserActionTrackings.Where(uat => uat.UserId == userId)
            .OrderByDescending(uat => uat.ActionTime)
            .Take(100)
            .ToListAsync(cancellationToken);
        }
    }
}
