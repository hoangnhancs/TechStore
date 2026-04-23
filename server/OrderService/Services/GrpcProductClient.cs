using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using OrderService.DTOs;
using ProductService.Grpc;

namespace OrderService.Services
{
    public class GrpcProductClient
    {
        private readonly ILogger<GrpcProductClient> _logger;
        private readonly GrpcProduct.GrpcProductClient _client;
        public GrpcProductClient(ILogger<GrpcProductClient> logger, GrpcProduct.GrpcProductClient client)
        {
            _logger = logger;
            _client = client;
        }
        public ReserveStockResponse ReserveStock (List<CreateOrderItemDto> createItems)
        {
            _logger.LogInformation("Reserve stock of product from gRPC service");
            
            var request = new ReserveStockRequest();
            request.Items.AddRange(createItems.Select(i => new ReserveStockItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }));

            try
            {
                var reply = _client.ReserveStock(request);
                return reply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC service: {Message}", ex.Message);
                return new ReserveStockResponse()
                {
                  Success = false,
                  ErrorMessage = "Failed to reserve stock: " + ex.Message  
                };
            }
        }
    }
}