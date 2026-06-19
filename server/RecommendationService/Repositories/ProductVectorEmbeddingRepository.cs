using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Polly;
using RecommendationService.Data;
using RecommendationService.Entities;
using RecommendationService.Repositories.Interface;
using System.Numerics;

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
            return await _dbContext.ProductVectorEmbeddings.FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        }

        public async Task<List<ProductVectorEmbedding>> GetProductVectorEmbeddingsByProductIds(HashSet<string> productIds, CancellationToken cancellationToken)
        {
            return await _dbContext.ProductVectorEmbeddings.Where(p => productIds.Contains(p.ProductId)).ToListAsync(cancellationToken);
        }

        public async Task<List<string>> GetTopSimilarProductsAsync(List<float> avgVector, int top, CancellationToken cancellationToken)
        {
            var embeddings = await _dbContext.ProductVectorEmbeddings
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);

            return embeddings
                .Select(x => new
                {
                    x.ProductId,
                    Score = CosineSimilarity(avgVector, x.Embedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(top)
                .Select(x => x.ProductId)
                .ToList();
        }

        public async Task UpdateProductVectorEmbedding(string productId, string vector)
        {
            var product = await _dbContext.ProductVectorEmbeddings.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product != null)
            {
                product.EmbeddingJson = vector;
            }
        }

        #region Helpers
        public static float CosineSimilarity(List<float> a, List<float> b)
        {
            if (a.Count != b.Count) throw new ArgumentException("Vectors must have the same length");

            float dot = 0f;
            float normA = 0f;
            float normB = 0f;

            for (int i = 0; i < a.Count; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return (float)(dot / (Math.Sqrt(normA) * Math.Sqrt(normB)));
        }
        #endregion
    }
}
