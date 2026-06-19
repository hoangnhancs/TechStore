using MongoDB.Entities;
using ProductService.Grpc;
using Recommendations.Grpc;
using SearchService.Entities;

namespace SearchService.Services
{
    public class GrpcRecommendationClient
    {
        private ILogger<GrpcProductClient> _logger;
        private readonly GrpcRecommendation.GrpcRecommendationClient _client;
        public GrpcRecommendationClient(ILogger<GrpcProductClient> logger, GrpcRecommendation.GrpcRecommendationClient client)
        {
            _logger = logger;
            _client = client;
        }
        public async Task<List<ProductItem>> GetSuggestProduct(string? userId, int numberTopProduct)
        {
            var request = new GetSuggestProductRequest { NumberOfProducts = numberTopProduct};

            if (!string.IsNullOrEmpty(userId))
            {
                request.UserId = userId;
            }
   

            try
            {
                var reply = await _client.GetSuggestProductAsync(request);
                _logger.LogInformation("Received {ProductCount} products from gRPC service", reply.ProductIds
                    .Count);

                var productIds = reply.ProductIds.ToList();
                var products = await DB.Find<ProductItem>().Match(x => productIds.Contains(x.ID))
                        .ExecuteAsync();
                foreach (ProductItem p in products)
                {
                    p.UpdateAttributeText();
                }
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC service: {Message}", ex.Message);
                return new List<ProductItem>();
            }
        }
    }
}
