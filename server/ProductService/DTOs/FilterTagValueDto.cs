using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTOs
{
    public class FilterTagValueDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = "";
        public int FilterTagId { get; set; }
    }
}