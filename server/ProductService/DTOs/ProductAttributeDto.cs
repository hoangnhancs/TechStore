using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class ProductAttributeDto
    {
        public int? Id { get; set; } 
        public required string Name { get; set; }
        public required string Value { get; set; }
        public string? ProductId { get; set; }
        public long? DisplayOrder { get; set; }
        public required string AttributeType { get; set; }
    }
}