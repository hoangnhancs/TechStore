using Grpc.Core;
using MediatR;
using Recommendations.Grpc;
using RecommendationService.Data;
using RecommendationService.Persistence;
using RecommendationService.Services.ProductVectorEmbedding;

namespace RecommendationService.Services
{
    public class GrpcRecommendationService : GrpcRecommendation.GrpcRecommendationBase
    {
        private readonly RecommandationSvcDbContext _dbContext;
        private readonly ILogger<GrpcRecommendationService> _logger;
        private readonly IMediator _mediator;
        private readonly IRecommandationUnitOfWork _unitOfWork;

        public GrpcRecommendationService(RecommandationSvcDbContext dbContext, ILogger<GrpcRecommendationService> logger, IMediator mediator, IRecommandationUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        public override async Task<GrpcSuggestProductResponse> GetSuggestProduct(GetSuggestProductRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Received gRPC request for top 10 sold products");
            var topProductsResponse = await _mediator.Send(new GetSuggestionProductQuery
            {
                UserId = request.HasUserId ? request.UserId : null,  //optional thì check như này cho chuẩn
                NumberTopProduct = request.NumberOfProducts
            });

            if (!topProductsResponse.IsSuccess)
            {
                _logger.LogError("Failed to get top products: {Error}", topProductsResponse.Error);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get product suggestions"));
            }

            else
            {
                var response = new GrpcSuggestProductResponse();
                var topProducts = topProductsResponse.Value;
                response.ProductIds.AddRange(topProducts);
                _logger.LogInformation("Returning {Count} products in gRPC response", topProducts?.Count);
                return response;
            }    
        }
    } 
}
