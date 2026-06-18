using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RecommendationService.DTOs;
using RecommendationService.Entities;
using RecommendationService.Services;

namespace RecommendationService.Data
{
    public class DbInitializer
    {
        public static async Task SeedData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<RecommandationSvcDbContext>();
                var productClient = services.GetRequiredService<ProductSvcHttpClient>();
                var vectorClient = services.GetRequiredService<VectorServiceClient>();
                var logger = services.GetRequiredService<ILogger<DbInitializer>>();

                // Check if already seeded
                if (context.ProductVectorEmbeddings.Any())
                {
                    logger.LogInformation("Database already seeded with {Count} embeddings", 
                        context.ProductVectorEmbeddings.Count());
                    return;
                }

                // Check VectorService health
                var isHealthy = await vectorClient.IsHealthyAsync();
                if (!isHealthy)
                {
                    logger.LogWarning("VectorService is not available. Skipping seed.");
                    return;
                }

                logger.LogInformation("Fetching products from ProductService...");
                var products = await productClient.GetItemsForSearchDb();
                
                if (products.Count == 0)
                {
                    logger.LogWarning("No products found in ProductService");
                    return;
                }

                logger.LogInformation("Found {Count} products. Generating embeddings...", products.Count);

                var embeddings = new List<ProductVectorEmbedding>();
                var processedCount = 0;
                var failedCount = 0;

                foreach (var product in products)
                {
                    try
                    {
                        // Map ProductItem to VectorService request
                        var request = new ProductEmbeddingRequest
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Description = product.Description ?? string.Empty,
                            OldPrice = product.OldPrice,
                            DiscountPercentage = (decimal)product.DiscountPercentage,
                            CategoryName = product.CategoryName,
                            BrandName = product.BrandName,
                            Tags = product.Attributes.Select(a => new ProductTag
                            {
                                Name = a.Name,
                                Value = a.Value
                            }).ToList()
                        };

                        // Generate embedding via VectorService
                        var response = await vectorClient.GenerateProductEmbeddingAsync(request);

                        if (response != null && response.Embedding.Count > 0)
                        {
                            embeddings.Add(new ProductVectorEmbedding
                            {
                                ProductId = response.ProductId,
                                IsProductDeleted = product.IsDeleted,
                                Embedding = response.Embedding
                                
                            });

                            processedCount++;
                            
                            if (processedCount % 10 == 0)
                            {
                                logger.LogInformation("Processed {Count}/{Total} products...", 
                                    processedCount, products.Count);
                            }
                        }
                        else
                        {
                            failedCount++;
                            logger.LogWarning("Failed to generate embedding for product: {ProductId}", product.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        logger.LogError(ex, "Error processing product: {ProductId}", product.Id);
                    }
                }

                // Save all embeddings to database
                if (embeddings.Count > 0)
                {
                    logger.LogInformation("Saving {Count} embeddings to database...", embeddings.Count);
                    await context.ProductVectorEmbeddings.AddRangeAsync(embeddings);
                    await context.SaveChangesAsync();
                    logger.LogInformation("✅ Successfully seeded {Success} embeddings. Failed: {Failed}", 
                        processedCount, failedCount);
                }
                else
                {
                    logger.LogWarning("No embeddings to save");
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<DbInitializer>>();
                logger.LogError(ex, "Error during database seeding");
                throw;
            }
        }
    }
}