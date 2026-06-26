using ReviewService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace ReviewService.Repositories.Interface
{
    public interface IReviewRepository : IBaseEFRepository<Review, string>
    {
        Task<List<Review>> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);
    }
}
