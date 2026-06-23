using Grpc.Core;
using ProductService.Grpc;

namespace RecommendationService.Services
{
    public class GrpcProductClient
    {
        private readonly ILogger<GrpcProductClient> _logger;
        private readonly GrpcProduct.GrpcProductClient _client; //generated client from proto file

        public GrpcProductClient(ILogger<GrpcProductClient> logger, GrpcProduct.GrpcProductClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<List<GrpcProductModel>> GetTop10SoldProduct(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting top 10 sold products from gRPC service");

                var response = await _client.GetTop10SoldProductAsync(
                    new GetTop10SoldProductRequest { },
                    deadline: DateTime.UtcNow.AddSeconds(30), // Timeout 5 seconds
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Received top 10 products");
                return response.Products.ToList();
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                _logger.LogWarning("Products not found");
                return new List<GrpcProductModel>();
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                _logger.LogError(ex, "Timeout calling gRPC service");
                throw new TimeoutException($"Timeout getting products from gRPC service", ex);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error calling gRPC service. Status: {Status}", ex.Status);
                throw new InvalidOperationException($"Error getting product from gRPC service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting products from gRPC service");
                throw;
            }
        }
    }
}
