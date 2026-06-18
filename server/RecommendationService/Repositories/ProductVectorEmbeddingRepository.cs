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

        public async Task<List<ProductVectorEmbedding>> GetTopRecommendedProductsAsync(string userId, int numberOfProducts, CancellationToken cancellationToken)
        {
            var productTrackingWeight = new Dictionary<string, float>();//weight of each product in the user tracking list
            var hashSetProductId = new HashSet<string>();
            var userTracking = await _dbContext.UserActionTrackings.Where(uat => uat.UserId == userId)
                .OrderByDescending(uat => uat.ActionTime)
                .Take(100)
                .ToListAsync(cancellationToken);
            foreach (var item in userTracking)
            {
                float weight = item.ActionType switch
                {
                    UserActionType.View => 1f,
                    UserActionType.AddToCart => 1.5f,
                    UserActionType.Purchase => 2f,
                    _ => 0f
                };
                if (weight == 0f) continue;
                if (productTrackingWeight.ContainsKey(item.ProductId))
                {
                    productTrackingWeight[item.ProductId] += weight;
                }
                else
                {
                    productTrackingWeight.Add(item.ProductId, weight);
                }
                hashSetProductId.Add(item.ProductId);
            }

            var productEmbedVectors = await _unitOfWork.ProductVectorEmbeddingRepository.GetProductVectorEmbeddingsByProductIds(hashSetProductId, cancellationToken);
            var inputVectors = new List<List<float>>();
            foreach (var item in productEmbedVectors)
            {
                var tmpVector = MultiplyWithWeight(item.Embedding, productTrackingWeight[item.ProductId]);
                inputVectors.Add(tmpVector);
            }
            var avgEmbedVector = ComputeAverageVector(inputVectors);
            var allVectorsWithProduct = await _unitOfWork.ProductVectorEmbeddingRepository.GetAll().Where(p => p.IsDeleted == false).ToListAsync(cancellationToken);

            var resultsVectors = allVectorsWithProduct
                .Select(p => new
                {
                    ProductId = p.ProductId,
                    Score = CosineSimilarity(avgEmbedVector, p.Embedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(10);
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
