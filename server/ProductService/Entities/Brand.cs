using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{
    public class Brand : BaseEntity<int>
    {
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public required string Name { get; set; }
        public required string ImageUrl { get; set; }    
    }
}