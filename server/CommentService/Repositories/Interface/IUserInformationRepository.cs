using CommentService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace CommentService.Repositories.Interface
{
    public interface IUserInformationRepository : IBaseEFRepository<UserInformation, int>
    {

    }
}
