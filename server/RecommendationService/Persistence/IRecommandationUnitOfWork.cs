using RecommendationService.Repositories.Interface;
using Shared.Core.EF.UnitOfWork;

namespace RecommendationService.Persistence
{
    public interface IRecommandationUnitOfWork : IUnitOfWork
    {
        IProductVectorEmbeddingRepository ProductVectorEmbeddingRepository { get; }
        IUserActionTrackingRepository UserActionTrackingRepository { get; }
    }
}
