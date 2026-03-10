using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class UpdateProductDto : UpsertProductDto
    {
        public IFormFile? MainImageFile { get; set; }
        public string? MainImageUrl { get; set; }
        public List<IFormFile> DetailImageFiles { get; set; } = new List<IFormFile>();
        public List<string> DetailImageUrls { get; set; } = new List<string>();
    }
}