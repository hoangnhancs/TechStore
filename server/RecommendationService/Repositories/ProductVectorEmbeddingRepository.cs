using Infrastructure.EF.Repositories;
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
    }
}
