using System.Text.Json.Serialization;

namespace RecommendationService.DTOs;

/// <summary>
/// Request to VectorService to generate product embedding
/// </summary>
public class ProductEmbeddingRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("old_price")]
    public decimal OldPrice { get; set; }

    [JsonPropertyName("discount_percentage")]
    public decimal DiscountPercentage { get; set; }

    [JsonPropertyName("category_name")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("brand_name")]
    public string? BrandName { get; set; }

    [JsonPropertyName("tags")]
    public List<ProductTag> Tags { get; set; } = new();
}

public class ProductTag
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Response from VectorService containing product embedding
/// </summary>
public class ProductEmbeddingResponse
{
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("embedding")]
    public List<float> Embedding { get; set; } = new();

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("dimension")]
    public int Dimension { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}

/// <summary>
/// Simple text embedding request
/// </summary>
public class TextEmbeddingRequest
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Text embedding response
/// </summary>
public class TextEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public List<float> Embedding { get; set; } = new();

    [JsonPropertyName("dimension")]
    public int Dimension { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}
