using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{
    public class ProductDisplayTag : BaseEntity<int>
    {
        public string DisplayTag { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public Product? Product { get; set; } 
    }
}