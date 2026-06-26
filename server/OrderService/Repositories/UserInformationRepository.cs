using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Entities;
using OrderService.Repositories.Interface;

namespace OrderService.Repositories
{
    public class UserInformationRepository : BaseEFRepository<UserInformation, int, OrderSvcDbContext>, IUserInformationRepository
    {
        public UserInformationRepository(OrderSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UserInformation>> GetByUserIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(u => userIds.Contains(u.UserId)).ToListAsync(cancellationToken);
        }
    }
}
