using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using ProductService.Grpc;

namespace CartService.Services
{
    public class GrpcProductClient : IDisposable
    {
        private readonly ILogger<GrpcProductClient> _logger;
        private readonly GrpcChannel _channel;
        private readonly GrpcProduct.GrpcProductClient _client;
        private bool _disposed = false;

        public GrpcProductClient(ILogger<GrpcProductClient> logger, IConfiguration config)
        {
            _logger = logger;
            var grpcAddress = config["GrpcProduct"] ?? throw new InvalidOperationException("GrpcProduct address is not configured");
            
            // Reuse channel for better performance
            _channel = GrpcChannel.ForAddress(grpcAddress, new GrpcChannelOptions
            {
                // Thêm các options cho production
                MaxReceiveMessageSize = 5 * 1024 * 1024, // 5MB
                MaxSendMessageSize = 5 * 1024 * 1024,
            });
            _client = new GrpcProduct.GrpcProductClient(_channel);
        }

        public async Task<GrpcProductModel?> GetProduct(string productId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting product from gRPC service with Id: {ProductId}", productId);

                var response = await _client.GetProductAsync(
                    new GetProductRequest { Id = productId },
                    deadline: DateTime.UtcNow.AddSeconds(5), // Timeout 5 seconds
                    cancellationToken: cancellationToken
                );

                if (response?.Product == null)
                {
                    _logger.LogWarning("No product found for ID: {ProductId}", productId);
                    return null;
                }

                _logger.LogInformation("Received product details for ID: {ProductId}", productId);
                return response.Product;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                _logger.LogWarning("Product not found for ID: {ProductId}", productId);
                return null;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                _logger.LogError(ex, "Timeout calling gRPC service for product ID: {ProductId}", productId);
                throw new TimeoutException($"Timeout getting product {productId} from gRPC service", ex);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error calling gRPC service for product ID: {ProductId}. Status: {Status}", productId, ex.Status);
                throw new InvalidOperationException($"Error getting product {productId} from gRPC service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting product from gRPC service for ID: {ProductId}", productId);
                throw;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _channel?.Dispose();
                _disposed = true;
            }
        }
    }
}