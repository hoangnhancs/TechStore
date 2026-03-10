using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ProductService;
using SearchService.Entities;

namespace SearchService.Services
{
    public class GrpcProductClient
    {
        private ILogger<GrpcProductClient> _logger;
        private readonly IConfiguration _config;
        public GrpcProductClient(ILogger<GrpcProductClient> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
        public List<ProductItem> GetProduct(DateTime? lastUpdated)
        {
            _logger.LogInformation("Getting products from gRPC service with last updated: {LastUpdated}", lastUpdated);
            var channel = GrpcChannel.ForAddress(_config["GrpcProduct"] ?? throw new InvalidOperationException("GrpcProduct address is not configured"));
            var client = new GrpcProduct.GrpcProductClient(channel);
            var request = new GetProductRequest
            {
                LastUpdated = lastUpdated.HasValue ? Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(lastUpdated.Value.ToUniversalTime()) : null
            };

            try
            {
                var reply = client.GetProduct(request);
                _logger.LogInformation("Received {ProductCount} products from gRPC service", reply.Product
                    .Count);
                return reply.Product.Select(p => new ProductItem
                {
                    ID = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    DiscountPercentage = p.DiscountPercentage,
                    CategoryId = p.CategoryId,
                    CategoryName = p.CategoryName,
                    CategoryDisplayName = p.CategoryDisplayName,
                    BrandId = p.BrandId,
                    BrandName = p.BrandName,
                    MainImageUrl = p.MainImageUrl,
                    UrlSlug = p.UrlSlung,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    IsNewArrival = p.IsNewArrival,
                    IsOnSale = p.IsOnSale,
                    DisplayTags = p.DisplayTags.ToList(),
                    AverageRating = p.AverageRating,
                    RatingCount = p.RatingCount,
                    TotalRatingStar = p.TotalRatingStar,
                    ProductFilterTagValues = p.ProductFilterTagValues.Select(pftv => new ProductFilterTagValueDto
                    {
                        Id = pftv.Id,
                        FilterTagId = pftv.FilterTagId,
                        ProductId = pftv.ProductId,
                        FilterTagValueId = pftv.FilterTagValueId
                    }).ToList(),
                    Attributes = p.Attributes.Select(a => new ProductAttributeDto
                    {
                        Name = a.Name,
                        Value = a.Value,
                        AttributeType = a.AttributeType,
                        DisplayOrder = a.DisplayOrder,
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC service: {Message}", ex.Message);
                return new List<ProductItem>();
            }
        }
    }
}