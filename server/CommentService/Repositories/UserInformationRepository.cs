using CommentService.Data;
using CommentService.Entities;
using CommentService.Repositories.Interface;
using Infrastructure.EF.Repositories;

namespace CommentService.Repositories
{
    public class UserInformationRepository : BaseEFRepository<UserInformation, int, CommentSvcDbContext>, IUserInformationRepository
    {
        public UserInformationRepository(CommentSvcDbContext dbContext) : base(dbContext)
        {
        }
    }
}
