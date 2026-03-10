using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using ProductService.Data;
using Microsoft.EntityFrameworkCore;
using Google.Protobuf.WellKnownTypes;

namespace ProductService.Services
{
    public class GrpcProductService : GrpcProduct.GrpcProductBase
    {
        private readonly ProductSvcDbContext _dbContext;
        private readonly ILogger<GrpcProductService> _logger;

        public GrpcProductService(ProductSvcDbContext dbContext, ILogger<GrpcProductService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override async Task<GrpcProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Received GetProducts request for last updated: {LastUpdated}", request.LastUpdated);

            var lastUpdatedDateTime = request.LastUpdated?.ToDateTime() ?? DateTime.MinValue;
            var products = await _dbContext.Products.Where(p => p.UpdatedAt > lastUpdatedDateTime)
                .Include(p => p.ProductFilterTagValues)
                .ThenInclude(pftv => pftv.FilterTagValue)
                .Include(p => p.Attributes)
                .Include(p => p.DisplayTags)
                .ToListAsync();

            if (products == null)
            {
                _logger.LogInformation("No products found updated after {LastUpdated}", lastUpdatedDateTime);
                return new GrpcProductResponse();
            }

            var response = new GrpcProductResponse();
            
            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                var grpcProduct = new GrpcProductModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = (long)p.Price,
                    OldPrice = (long)p.OldPrice,
                    DiscountPercentage = (float)p.DiscountPercentage,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    CategoryDisplayName = p.Category?.DisplayName ?? string.Empty,
                    BrandId = p.BrandId,
                    BrandName = p.Brand?.Name ?? string.Empty,
                    MainImageUrl = p.MainImageUrl,
                    UrlSlung = p.UrlSlug,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    IsNewArrival = p.IsNewArrival,
                    IsOnSale = p.IsOnSale,
                    AverageRating = (double)p.AverageRating,
                    RatingCount = p.RatingCount,
                    TotalRatingStar = p.TotalRatingStar,
                    UnitSold = p.UnitSold,
                    CreatedAt = Timestamp.FromDateTime(p.CreatedAt.ToUniversalTime()),
                    UpdatedAt = Timestamp.FromDateTime(p.UpdatedAt.ToUniversalTime())
                };

                // Add Attributes (read-only collection)
                if (p.Attributes != null)
                {
                    grpcProduct.Attributes.AddRange(p.Attributes.Select(a => new GrpcProductAttributeDto
                    {
                        Name = a.Name,
                        Value = a.Value,
                        AttributeType = a.AttributeType,
                        DisplayOrder = a.DisplayOrder
                    }));
                }

                // Add DisplayTags (read-only collection)
                if (p.DisplayTags != null)
                {
                    grpcProduct.DisplayTags.AddRange(p.DisplayTags.Select(dt => dt.DisplayTag));
                }

                if (p.ProductFilterTagValues != null)
                {
                    grpcProduct.ProductFilterTagValues.AddRange(p.ProductFilterTagValues.Select(pftv => new GrpcProductFilterTagValueDto
                    {
                        Id = pftv.Id,
                        FilterTagValueId = pftv.FilterTagValueId,
                        FilterTagId = pftv.FilterTagValue == null ? 0 : pftv.FilterTagValue.FilterTagId,
                        ProductId = pftv.ProductId
                    }));
                }

                response.Product.Add(grpcProduct);
            }
            return response;
        }
    }
}