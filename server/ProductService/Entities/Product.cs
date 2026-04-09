using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities;

/// <summary>
/// Product Entity - Option 1: Extend BaseEntity (recommended)
/// BaseEntity cung cấp: Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
/// </summary>
public class Product : BaseEntity<string>
{
    // Id, CreatedAt, UpdatedAt được kế thừa từ BaseEntity
    
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required decimal OldPrice { get; set; }
    public required decimal Price { get; set; }
    public required decimal DiscountPercentage { get; set; }
    public required int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int BrandId { get; set; }
    public Brand? Brand { get; set; }
    public required string MainImageUrl { get; set; }
    public required string MainImagePublicId { get; set; }
    public required int QuantityInStock { get; set; }
    public required int ReservedQuantity { get; set; } = 0;
    public string? UrlSlug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool IsNewArrival { get; set; } = false;
    public bool IsOnSale { get; set; } = false;
    public List<ProductDisplayTag> DisplayTags { get; set; } = [];
    public List<ProductFilterTagValue> ProductFilterTagValues { get; set; } = [];
    public decimal AverageRating { get; set; } = 0;
    public int RatingCount { get; set; } = 0;
    public int TotalRatingStar { get; set; } = 0;
    public List<ProductImage> DetailImages { get; set; } = [];
    public List<ProductAttribute> Attributes { get; set; } = [];
    public int UnitSold { get; set; } = 0;

    // Constructor
    public Product() : base()
    {
        Id = Guid.NewGuid().ToString();
    }

    // Domain methods (Business logic)
    public void UpdatePrice(decimal newPrice, decimal oldPrice, decimal discountPercentage)
    {
        Price = newPrice;
        OldPrice = oldPrice;
        DiscountPercentage = discountPercentage;
        IsOnSale = discountPercentage > 0;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
        QuantityInStock += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
        if (QuantityInStock < quantity)
            throw new InvalidOperationException("Insufficient stock");
            
        QuantityInStock -= quantity;
        UnitSold += quantity;
    }

    public void UpdateRating(int newRating)
    {
        if (newRating < 1 || newRating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(newRating));
            
        TotalRatingStar += newRating;
        RatingCount++;
        AverageRating = (decimal)TotalRatingStar / RatingCount;
    }
}
