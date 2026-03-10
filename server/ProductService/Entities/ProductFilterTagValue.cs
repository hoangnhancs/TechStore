using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{
    public class ProductFilterTagValue : BaseEntity<int>
    {
        // [Key]
        // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // public int Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public Product? Product { get; set; }
        public int FilterTagValueId { get; set; }
        public FilterTagValue? FilterTagValue { get; set; }
    }
}