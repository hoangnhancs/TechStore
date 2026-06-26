using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.Entities;
using ReviewService.Repositories.Interface;

namespace ReviewService.Repositories
{
    public class ReviewRepository : BaseEFRepository<Review, string, ReviewSvcDbContext>, IReviewRepository
    {
        public ReviewRepository(ReviewSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Review>> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
