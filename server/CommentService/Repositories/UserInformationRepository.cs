using CommentService.Data;
using CommentService.Entities;
using CommentService.Repositories.Interface;
using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Repositories
{
    public class UserInformationRepository : BaseEFRepository<UserInformation, int, CommentSvcDbContext>, IUserInformationRepository
    {
        public UserInformationRepository(CommentSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UserInformation>> GetByUserIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(u => userIds.Contains(u.UserId)).ToListAsync(cancellationToken);
        }
    }
}
