using RecommendationService.Repositories.Interface;
using Shared.Core.EF.UnitOfWork;

namespace RecommendationService.Persistence
{
    public interface IRecommendationUnitOfWork : IUnitOfWork
    {
        IProductVectorEmbeddingRepository ProductVectorEmbeddingRepository { get; }
        IUserActionTrackingRepository UserActionTrackingRepository { get; }
    }
}
