using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{
    public class Category : BaseEntity<int>
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<Product> Products { get; set; } = [];
        public List<Brand> Brands { get; set; } = [];
        public List<FilterTag> FilterTags { get; set; } = [];    
    }
}