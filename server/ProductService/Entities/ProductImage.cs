using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{
    public class ProductImage : BaseEntity<string>
    {
        public string ImageUrl { get; set; } = string.Empty;
        // chỉ dùng khi ảnh lưu trên Cloudinary
        //khi khởi tạo thì không có PublicId
        public required string PublicId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public Product? Product { get; set; } 
        public ProductImage() : base(Guid.NewGuid().ToString()) { }
    }
}