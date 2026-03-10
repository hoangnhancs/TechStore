using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Contract
{
    public class ProductUpdated
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long OldPrice { get; set; }
        public long Discount { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public string BrandId { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }
        public List<ProductFTV> ProductFilterTagValues { get; set; } = new List<ProductFTV>();
        public List<ProductAttr> Attributes { get; set; } = new List<ProductAttr>();
        public MainImageDto MainImageInput { get; set; } = null!;
        public DetailImageDto? DetailImageInputs { get; set; }
    }

        public class MainImageDto
    {
        public IFormFile? File { get; set; } // image moi
        public string? Url { get; set; } //keep old image
    }

    public class DetailImageDto
    {
        public List<IFormFile> Files { get; set; } = []; // images moi
        public List<string> Urls { get; set; } = []; //keep old images
    }
}