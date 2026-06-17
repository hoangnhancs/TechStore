using RecommendationService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace RecommendationService.Repositories.Interface
{
    public interface IUserActionTrackingRepository : IBaseEFRepository<UserActionTracking, int>
    {
        Task<List<UserActionTracking>> GetUserActionTrackingByUserId(string userId, CancellationToken cancellationToken);
    }
}
