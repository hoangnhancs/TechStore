using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class BannerImageDto
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string PublicId { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}