using RecommendationService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace RecommendationService.Repositories.Interface
{
    public interface IProductVectorEmbeddingRepository : IBaseEFRepository<ProductVectorEmbedding, int>
    {
    }
}
