using OrderService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace OrderService.Repositories.Interface
{
    public interface IUserInformationRepository : IBaseEFRepository<UserInformation, int>
    {
        Task<List<UserInformation>> GetByUserIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default);
    }
}
