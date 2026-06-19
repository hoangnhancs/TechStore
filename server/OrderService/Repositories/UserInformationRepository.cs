using Infrastructure.EF.Repositories;
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
    }
}
