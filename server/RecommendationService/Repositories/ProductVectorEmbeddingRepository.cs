using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using RecommendationService.Data;
using RecommendationService.Entities;
using RecommendationService.Repositories.Interface;

namespace RecommendationService.Repositories
{
    public class ProductVectorEmbeddingRepository : BaseEFRepository<ProductVectorEmbedding, int, RecommandationSvcDbContext>, IProductVectorEmbeddingRepository
    {
        public ProductVectorEmbeddingRepository(RecommandationSvcDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<ProductVectorEmbedding?> GetProductVectorEmbeddingByProductId(string productId, CancellationToken cancellationToken)
        {
            return await _dbContext.ProductVectorEmbeddings.FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        }

        public async Task<List<ProductVectorEmbedding>> GetProductVectorEmbeddingsByProductIds(HashSet<string> productIds, CancellationToken cancellationToken)
        {
            return await _dbContext.ProductVectorEmbeddings.Where(p => productIds.Contains(p.ProductId)).ToListAsync(cancellationToken);
        }

        public async Task UpdateProductVectorEmbedding(string productId, string vector)
        {
            var product = await _dbContext.ProductVectorEmbeddings.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product != null)
            {
                product.EmbeddingJson = vector;
            }
        }
    }
}
