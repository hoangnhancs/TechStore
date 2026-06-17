using Infrastructure.EF.UnitOfWork;
using RecommendationService.Data;
using RecommendationService.Repositories;
using RecommendationService.Repositories.Interface;

namespace RecommendationService.Persistence
{
    public class RecommandationUnitOfWork : UnitOfWork<RecommandationSvcDbContext>, IRecommandationUnitOfWork
    {
        private IProductVectorEmbeddingRepository? _productVectorEmbeddingRepository;
        private IUserActionTrackingRepository? _userActionTrackingRepository;
        public IProductVectorEmbeddingRepository ProductVectorEmbeddingRepository =>
            _productVectorEmbeddingRepository ??= new ProductVectorEmbeddingRepository(_dbContext);
        public IUserActionTrackingRepository UserActionTrackingRepository =>
            _userActionTrackingRepository ??= new UserActionTrackingRepository(_dbContext);
        public RecommandationUnitOfWork(RecommandationSvcDbContext context) : base(context)
        {
        }
    }
}
