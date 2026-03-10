using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{
    public class FilterTagValue : BaseEntity<int>
    {
        public string Value { get; set; } = "";
        public int FilterTagId { get; set; }
        public FilterTag? FilterTag { get; set; }
    }
}