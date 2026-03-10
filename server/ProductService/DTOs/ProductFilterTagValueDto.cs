using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class ProductFilterTagValueDto
    {
        public int? Id { get; set; }
        public int FilterTagId { get; set; }
        public required int FilterTagValueId { get; set; }
        public string ProductId { get; set; } = string.Empty;
    }
}