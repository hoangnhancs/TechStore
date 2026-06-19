using RecommendationService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace RecommendationService.Repositories.Interface
{
    public interface IProductVectorEmbeddingRepository : IBaseEFRepository<ProductVectorEmbedding, int>
    {
        Task UpdateProductVectorEmbedding(string productId, string vector);
        Task<ProductVectorEmbedding?> GetProductVectorEmbeddingByProductId(string productId, CancellationToken cancellationToken);
        Task<List<ProductVectorEmbedding>> GetProductVectorEmbeddingsByProductIds(HashSet<string> productIds, CancellationToken cancellationToken); // List<ProductVectorEmbedding> GetProductVectorEmbeddingsAsync(CancellationToken cancellationToken);
        Task<List<ProductVectorEmbedding>> GetActiveEmbeddingsAsync(CancellationToken cancellationToken);
        Task<List<string>> GetTopSimilarProductsAsync(List<float> avgVector, int top, CancellationToken cancellationToken);
    }
}
