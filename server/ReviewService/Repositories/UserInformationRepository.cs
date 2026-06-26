using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.Entities;
using ReviewService.Repositories.Interface;

namespace ReviewService.Repositories
{
    public class UserInformationRepository : BaseEFRepository<UserInformation, int, ReviewSvcDbContext>, IUserInformationRepository
    {
        public UserInformationRepository(ReviewSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UserInformation>> GetByUserIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(u => userIds.Contains(u.UserId)).ToListAsync(cancellationToken);
        }
    }
}