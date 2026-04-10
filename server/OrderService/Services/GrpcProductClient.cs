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
        private readonly GrpcChannel _channel;
        private readonly GrpcProduct.GrpcProductClient _client;
        private readonly IConfiguration _config;    
        public GrpcProductClient(ILogger<GrpcProductClient> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            var grpcAddress = _config["GrpcProduct"] ?? throw new InvalidOperationException("GrpcProduct address is not configured");
            
            // Reuse channel for better performance
            _channel = GrpcChannel.ForAddress(grpcAddress, new GrpcChannelOptions
            {
                // Thêm các options cho production
                MaxReceiveMessageSize = 5 * 1024 * 1024, // 5MB
                MaxSendMessageSize = 5 * 1024 * 1024,
            });
            _client = new GrpcProduct.GrpcProductClient(_channel);
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