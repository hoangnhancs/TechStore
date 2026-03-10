using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class CreateProductDto : UpsertProductDto
    {
        public required IFormFile MainImageFile { get; set; }
        public List<IFormFile> DetailImageFiles { get; set; } = new List<IFormFile>();
    }
}