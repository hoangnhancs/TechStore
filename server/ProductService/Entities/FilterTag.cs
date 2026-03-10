using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{
    public class FilterTag : BaseEntity<int>
    {
        public string Name { get; set; } = "";
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public List<FilterTagValue> Values { get; set; } = [];
    }
}