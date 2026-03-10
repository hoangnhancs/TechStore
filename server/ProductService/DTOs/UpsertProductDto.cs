using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class UpsertProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal OldPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; } 
        public int BrandId { get; set; } 
        public int QuantityInStock { get; set; }
        public List<ProductFilterTagValueDto> ProductFilterTagValues { get; set; } = new List<ProductFilterTagValueDto>();
        public List<ProductAttributeDto> Attributes { get; set; } = new List<ProductAttributeDto>();  
    }
    // public class ProductAttrDto
    // {
    //     public string Value { get; set; } = null!;
    //     public string Name { get; set; } = null!;
    // }

    // public class ProductAttrGroupDto
    // {
    //     public string GroupName { get; set; } = null!;
    //     public List<ProductAttrDto> Attributes { get; set; } = new();
    // }
}