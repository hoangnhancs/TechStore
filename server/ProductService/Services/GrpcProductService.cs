using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public override async Task<GrpcUpdatedProductResponse> GetUpdatedProduct(GetUpdatedProductRequest request, ServerCallContext context)
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
                return new GrpcUpdatedProductResponse();
            }

            var response = new GrpcUpdatedProductResponse();
            
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
                    UpdatedAt = Timestamp.FromDateTime(p.UpdatedAt.ToUniversalTime()),
                    QuantityInStock = p.QuantityInStock
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

        public override async Task<GrpcProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Received GetProduct request for product ID: {ProductId}", request.Id);

            var p = await _dbContext.Products.Where(p => p.Id == request.Id)
                .Include(p => p.ProductFilterTagValues)
                .ThenInclude(pftv => pftv.FilterTagValue)
                .Include(p => p.Attributes)
                .Include(p => p.DisplayTags)
                .FirstOrDefaultAsync();

            if (p == null)
            {
                _logger.LogInformation("Product with ID {ProductId} not found", request.Id);
                return new GrpcProductResponse();
            }

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
                QuantityInStock = p.QuantityInStock,
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
            return new GrpcProductResponse
            {
                Product = grpcProduct
            };
        }

        public override async Task<GrpcProductsResponse> GetProducts(GetProductsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Received GetProducts request for {ProductIdsCount} product IDs", request.Ids.Count);

            var products = await _dbContext.Products.Where(p => request.Ids.Contains(p.Id))
                .Include(p => p.ProductFilterTagValues)
                .ThenInclude(pftv => pftv.FilterTagValue)
                .Include(p => p.Attributes)
                .Include(p => p.DisplayTags)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToListAsync();

            var response = new GrpcProductsResponse();

            foreach (var p in products)
            {
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
                    UpdatedAt = Timestamp.FromDateTime(p.UpdatedAt.ToUniversalTime()),
                    QuantityInStock = p.QuantityInStock
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
                response.Products.Add(grpcProduct);
            }
            return response;
        }

        /// <summary>
        /// Reserve stock for order items with atomic check-and-update
        /// This will decrease the QuantityInStock for each product atomically
        /// </summary>
        public override async Task<ReserveStockResponse> ReserveStock(ReserveStockRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Received ReserveStock request for {ItemCount} items", request.Items.Count);

            var response = new ReserveStockResponse
            {
                Success = true
            };

            // Start a transaction to ensure atomicity across multiple products
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in request.Items)
                {
                    var result = new StockReservationResult
                    {
                        ProductId = item.ProductId,
                        RequestedQuantity = item.Quantity
                    };

                    // First, get product details for error messages
                    var product = await _dbContext.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Product not found";
                        result.AvailableQuantity = 0;
                        result.ProductName = "Unknown";
                        response.Success = false;
                        _logger.LogWarning("Product {ProductId} not found during stock reservation", item.ProductId);
                        response.Results.Add(result);
                        continue;
                    }

                    // Atomic check-and-update: Only update if stock is sufficient
                    // This prevents race conditions
                    var rowsAffected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE ""Products"" 
                           SET ""QuantityInStock"" = ""QuantityInStock"" - {item.Quantity}
                           WHERE ""Id"" = {item.ProductId} 
                           AND ""QuantityInStock"" >= {item.Quantity}");

                    if (rowsAffected == 0)
                    {
                        // Either product not found or insufficient stock
                        // Re-fetch to get current stock for error message
                        var currentProduct = await _dbContext.Products
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                        result.Success = false;
                        result.ProductName = product.Name;
                        result.AvailableQuantity = currentProduct?.QuantityInStock ?? 0;
                        result.ErrorMessage = $"Insufficient stock. Available: {result.AvailableQuantity}, Requested: {item.Quantity}";
                        response.Success = false;
                        _logger.LogWarning("Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}", 
                            item.ProductId, result.AvailableQuantity, item.Quantity);
                    }
                    else
                    {
                        // Successfully reserved
                        result.Success = true;
                        result.ProductName = product.Name;
                        result.AvailableQuantity = product.QuantityInStock - item.Quantity;
                        result.ErrorMessage = string.Empty;
                        _logger.LogInformation("Atomically reserved {Quantity} units of product {ProductId} ({ProductName})", 
                            item.Quantity, item.ProductId, product.Name);
                    }

                    response.Results.Add(result);
                }

                if (response.Success)
                {
                    // Commit transaction if all reservations were successful
                    await transaction.CommitAsync();
                    _logger.LogInformation("Stock reservation completed successfully for {ItemCount} items", request.Items.Count);
                }
                else
                {
                    // Rollback if any reservation failed
                    await transaction.RollbackAsync();
                    response.ErrorMessage = "Stock reservation failed for one or more items";
                    _logger.LogWarning("Stock reservation failed, transaction rolled back");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.ErrorMessage = $"Error during stock reservation: {ex.Message}";
                _logger.LogError(ex, "Error during stock reservation");
            }

            return response;
        }
        public async override Task<GrpcProductsResponse> GetTop10SoldProduct(GetTop10SoldProductRequest request, ServerCallContext context)
        {
            var products = await _dbContext.Products.Where(p => p.IsActive)
            .OrderByDescending(p => p.UnitSold)
            .Take(10)
            .ToListAsync();

            var response = new GrpcProductsResponse();

            foreach (var p in products)
            {
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
                    UpdatedAt = Timestamp.FromDateTime(p.UpdatedAt.ToUniversalTime()),
                    QuantityInStock = p.QuantityInStock
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
                response.Products.Add(grpcProduct);
            }
            return response;
        }
    }
}