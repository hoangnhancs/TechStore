using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{

    public class ProductAttribute : BaseEntity<int>
    {
        // [Key]
        // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public Product? Product { get; set; }
        public long DisplayOrder { get; set; } 
        public string AttributeType { get; set; } = string.Empty; 
    }
}