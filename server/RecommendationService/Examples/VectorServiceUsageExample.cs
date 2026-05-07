using RecommendationService.DTOs;
using RecommendationService.Services;

namespace RecommendationService.Examples;

/// <summary>
/// Example usage of VectorServiceClient
/// </summary>
public class VectorServiceUsageExample
{
    private readonly VectorServiceClient _vectorClient;
    private readonly ILogger<VectorServiceUsageExample> _logger;

    public VectorServiceUsageExample(
        VectorServiceClient vectorClient,
        ILogger<VectorServiceUsageExample> logger)
    {
        _vectorClient = vectorClient;
        _logger = logger;
    }

    /// <summary>
    /// Example: Generate embedding for a product and store in database
    /// </summary>
    public async Task ProcessProductAsync(string productId, string productName, decimal price)
    {
        // 1. Create request matching VectorService API
        var request = new ProductEmbeddingRequest
        {
            Id = productId,
            Name = productName,
            Description = "Smart TV 4K HDR",
            OldPrice = price,
            DiscountPercentage = 10,
            CategoryName = "Tivi",
            BrandName = "Samsung",
            Tags = new List<ProductTag>
            {
                new() { Name = "Kich co", Value = "55 inch" },
                new() { Name = "Do phan giai", Value = "4K" }
            }
        };

        // 2. Call VectorService to generate embedding
        var response = await _vectorClient.GenerateProductEmbeddingAsync(request);

        if (response != null)
        {
            _logger.LogInformation(
                "Generated {Dimension}-dim embedding for product {ProductId}",
                response.Dimension,  // 384
                response.ProductId
            );

            // 3. Store embedding in RecommendationService's database
            // Example:
            // await _dbContext.ProductVectors.AddAsync(new ProductVector
            // {
            //     ProductId = response.ProductId,
            //     Embedding = response.Embedding.ToArray(),
            //     Text = response.Text,
            //     CreatedAt = DateTime.UtcNow
            // });
            // await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Stored embedding with {Count} dimensions", response.Embedding.Count);
        }
    }

    /// <summary>
    /// Example: Search similar products using text query
    /// </summary>
    public async Task SearchSimilarProductsAsync(string searchQuery)
    {
        // 1. Generate embedding for search query
        var queryEmbedding = await _vectorClient.GenerateTextEmbeddingAsync(searchQuery);

        if (queryEmbedding != null)
        {
            _logger.LogInformation("Generated search embedding with dimension: {Dim}", queryEmbedding.Dimension);

            // 2. Use embedding to find similar products in database
            // Example with pgvector:
            // var similar = await _dbContext.ProductVectors
            //     .OrderBy(p => p.Embedding.CosineDistance(queryEmbedding.Embedding))
            //     .Take(10)
            //     .ToListAsync();

            // Or manual cosine similarity:
            // var allProducts = await _dbContext.ProductVectors.ToListAsync();
            // var ranked = allProducts
            //     .Select(p => new
            //     {
            //         ProductId = p.ProductId,
            //         Similarity = CosineSimilarity(queryEmbedding.Embedding, p.Embedding)
            //     })
            //     .OrderByDescending(x => x.Similarity)
            //     .Take(10);
        }
    }

    /// <summary>
    /// Example: Batch process multiple products efficiently
    /// </summary>
    public async Task BatchProcessProductsAsync(List<string> productTexts)
    {
        // Generate embeddings for multiple products at once (more efficient)
        var embeddings = await _vectorClient.GenerateBatchEmbeddingsAsync(productTexts);

        if (embeddings != null)
        {
            _logger.LogInformation("Generated {Count} embeddings in batch", embeddings.Count);

            // Store all embeddings
            // for (int i = 0; i < embeddings.Count; i++)
            // {
            //     await StoreEmbeddingAsync(productTexts[i], embeddings[i]);
            // }
        }
    }

    /// <summary>
    /// Helper: Calculate cosine similarity between two vectors
    /// </summary>
    private static double CosineSimilarity(List<float> vector1, float[] vector2)
    {
        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        for (int i = 0; i < vector1.Count; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0;

        return dotProduct / (magnitude1 * magnitude2);
    }
}
