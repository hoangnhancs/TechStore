using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class ProductImageDto
    {
        public required string Id { get; set; } = string.Empty;
        public required string ImageUrl { get; set; }
        public string? PublicId { get; set; }
        public required string ProductId { get; set; }
    }
}