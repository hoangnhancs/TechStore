using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using ProductService.Grpc;

namespace CartService.Services
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

        public async Task<List<GrpcProductModel>> GetProducts(List<string> productIds, CancellationToken cancellationToken = default)
        {
            var products = new List<GrpcProductModel>();

            try
            {
                var response = await _client.GetProductsAsync(
                    new GetProductsRequest { Ids = { productIds } },
                    deadline: DateTime.UtcNow.AddSeconds(5),
                    cancellationToken: cancellationToken
                );
                products.AddRange(response.Products);
                _logger.LogInformation("Received {Count} products from gRPC service", products.Count);
                return products;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                _logger.LogWarning("Products not found");
                return products;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                _logger.LogError(ex, "Timeout calling gRPC service for products");
                throw new TimeoutException("Timeout getting products from gRPC service", ex);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error calling gRPC service for products. Status: {Status}", ex.Status);
                throw new InvalidOperationException("Error getting products from gRPC service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting products from gRPC service");
                throw;
            }
        }
    }
}