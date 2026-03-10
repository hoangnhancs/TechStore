using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace SearchService.Entities
{
    public class ProductItem : Entity
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public required long OldPrice { get; set; }
        public required long Price { get; set; }
        public required float DiscountPercentage { get; set; }
        public required int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryDisplayName { get; set; }
        // public string? Category { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public required string MainImageUrl { get; set; }
        // chỉ dùng khi ảnh lưu trên Cloudinary
        // public string? MainImagePublicId { get; set; }
        // public required int QuantityInStock { get; set; }
        public string? UrlSlug { get; set; } 
        // public string? MetaTitle { get; set; }
        // public string? MetaDescription { get; set; }
        // public string? MetaKeywords { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsNewArrival { get; set; } = false;
        public bool IsOnSale { get; set; } = false;
        public List<string> DisplayTags { get; set; } = [];
        public double AverageRating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
        public int TotalRatingStar { get; set; } = 0;
        // public List<Review> Reviews { get; set; } = [];
        public List<ProductFilterTagValueDto> ProductFilterTagValues { get; set; } = new List<ProductFilterTagValueDto>();
        public List<ProductAttributeDto> Attributes { get; set; } = new List<ProductAttributeDto>();
        public List<string> AttributeText { get; set; } = [];
        public int UnitSold { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public void UpdateAttributeText()
        {
            AttributeText = Attributes
                .Select(a => $"{a.Name}: {a.Value}")
                .ToList();

            UpdatedAt = DateTime.UtcNow;
        }
        public bool IsDeleted { get; set; } = false;
    }

    public class ProductFilterTagValueDto
    {
        public int? Id { get; set; }
        public int FilterTagId { get; set; }
        public required int FilterTagValueId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
    }

    public class ProductAttributeDto
    {
        public required string Name { get; set; }
        public required string Value { get; set; }
        public string? ProductId { get; set; }
        public long? DisplayOrder { get; set; }
        public required string AttributeType { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}