using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class ProductDto
    {
        public string? Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public required decimal OldPrice { get; set; }
        public required decimal Price { get; set; }
        public required long DiscountPercentage { get; set; }
        public required int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryDisplayName { get; set; }
        public string BrandId { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public required int QuantityInStock { get; set; }
        public string MainImageUrl { get; set; } = string.Empty;
        public string? MainImagePublicId { get; set; }
        public decimal AverageRating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
        public int UnitSold { get; set; } = 0;
        public string? UrlSlug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsNewArrival { get; set; } = false;
        public bool IsOnSale { get; set; } = false;
        public List<ProductImageDto>? DetailImages { get; set; } = [];
        public List<ProductFilterTagValueDto>? ProductFilterTagValues { get; set; } = [];
        public List<string>? DisplayTags { get; set; } = [];
        public List<ProductAttributeDto>? Attributes { get; set; } = [];
        // public List<ReviewDto>? Reviews { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}