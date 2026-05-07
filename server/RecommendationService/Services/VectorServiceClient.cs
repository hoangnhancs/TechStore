using System.Net.Http.Json;
using RecommendationService.DTOs;

namespace RecommendationService.Services;

/// <summary>
/// HTTP client for VectorService - generates ML embeddings
/// </summary>
public class VectorServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VectorServiceClient> _logger;
    private readonly string _baseUrl;

    public VectorServiceClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<VectorServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["VectorServiceUrl"] ?? "http://localhost:8000";
        _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    /// <summary>
    /// Generate embedding for a product
    /// </summary>
    public async Task<ProductEmbeddingResponse?> GenerateProductEmbeddingAsync(
        ProductEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating embedding for product: {ProductId}", request.Id);

            var response = await _httpClient.PostAsJsonAsync(
                "/products/embed",
                request,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ProductEmbeddingResponse>(
                cancellationToken: cancellationToken
            );

            if (result != null)
            {
                _logger.LogInformation(
                    "Successfully generated {Dimension}-dim embedding for product: {ProductId}",
                    result.Dimension,
                    result.ProductId
                );
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling VectorService for product: {ProductId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for product: {ProductId}", request.Id);
            throw;
        }
    }

    /// <summary>
    /// Generate embedding from simple text (for search queries)
    /// </summary>
    public async Task<TextEmbeddingResponse?> GenerateTextEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating embedding for text: {Text}", text.Substring(0, Math.Min(50, text.Length)));

            var request = new TextEmbeddingRequest { Text = text };

            var response = await _httpClient.PostAsJsonAsync(
                "/embed",
                request,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TextEmbeddingResponse>(
                cancellationToken: cancellationToken
            );

            if (result != null)
            {
                _logger.LogInformation("Successfully generated {Dimension}-dim text embedding", result.Dimension);
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling VectorService for text embedding");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text embedding");
            throw;
        }
    }

    /// <summary>
    /// Batch generate embeddings for multiple texts
    /// </summary>
    public async Task<List<List<float>>?> GenerateBatchEmbeddingsAsync(
        List<string> texts,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating batch embeddings for {Count} texts", texts.Count);

            var response = await _httpClient.PostAsJsonAsync(
                "/batch/embed",
                texts,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<BatchEmbeddingResponse>(
                cancellationToken: cancellationToken
            );

            if (result != null)
            {
                _logger.LogInformation(
                    "Successfully generated {Count} embeddings with dimension {Dimension}",
                    result.Count,
                    result.Dimension
                );
                return result.Embeddings;
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling VectorService for batch embeddings");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch embeddings");
            throw;
        }
    }

    /// <summary>
    /// Check if VectorService is healthy
    /// </summary>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "VectorService health check failed");
            return false;
        }
    }
}

// Helper DTO for batch response
internal class BatchEmbeddingResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("count")]
    public int Count { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("embeddings")]
    public List<List<float>> Embeddings { get; set; } = new();

    [System.Text.Json.Serialization.JsonPropertyName("dimension")]
    public int Dimension { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}
