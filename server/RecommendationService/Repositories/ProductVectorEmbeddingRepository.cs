using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
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

        public async Task<List<ProductVectorEmbedding>> GetActiveEmbeddingsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.ProductVectorEmbeddings
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<ProductVectorEmbedding?> GetProductVectorEmbeddingByProductId(string productId, CancellationToken cancellationToken)
        {
            return await _dbContext.ProductVectorEmbeddings
                .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        }

        public async Task<List<ProductVectorEmbedding>> GetProductVectorEmbeddingsByProductIds(HashSet<string> productIds, CancellationToken cancellationToken)
        {
            return await _dbContext.ProductVectorEmbeddings
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<string>> GetTopSimilarProductsAsync(List<float> avgVector, int top, CancellationToken cancellationToken)
        {
            var queryVector = new Vector(avgVector.ToArray());
            return await _dbContext.ProductVectorEmbeddings
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Embedding.CosineDistance(queryVector))
                .Take(top)
                .Select(x => x.ProductId)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateProductVectorEmbedding(string productId, List<float> embedding)
        {
            var product = await _dbContext.ProductVectorEmbeddings
                .FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product != null)
                product.Embedding = new Vector(embedding.ToArray());
        }
    }
}
