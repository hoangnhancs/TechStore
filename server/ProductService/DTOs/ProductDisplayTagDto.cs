using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class ProductDisplayTagDto
    {
        public int Id { get; set; }
        public string DisplayTag { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty; 
    }
}