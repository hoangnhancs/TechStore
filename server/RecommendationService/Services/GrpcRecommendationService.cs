using Grpc.Core;
using MediatR;
using Recommendations.Grpc;
using RecommendationService.Data;
using RecommendationService.Persistence;

namespace RecommendationService.Services
{
    public class GrpcRecommendationService : GrpcRecommendation.GrpcRecommendationBase
    {
        private readonly RecommandationSvcDbContext _dbContext;
        private readonly ILogger<GrpcRecommendationService> _logger;
        private readonly IMediator _mediator;
        private readonly RecommandationUnitOfWork _unitOfWork;

        public GrpcRecommendationService(RecommandationSvcDbContext dbContext, ILogger<GrpcRecommendationService> logger, IMediator mediator, RecommandationUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        public override async Task<GrpcSuggestProductResponse> GetSuggestProduct(GetSuggestProductRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Received gRPC request for top 10 sold products");
            var topProducts = await _unitOfWork.ProductVectorEmbeddingRepository.GetTopRecommendedProductsAsync(new HashSet<string> { request.UserId }, context.CancellationToken);
            var response = new GrpcSuggestProductResponse();
            response.ProductIds.AddRange(topProducts.Select(p => p.ProductId));
            _logger.LogInformation("Returning {Count} products in gRPC response", topProducts.Count);
            return response;
        }
    } 
}
