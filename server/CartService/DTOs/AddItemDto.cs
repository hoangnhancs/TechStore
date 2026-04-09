using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CartService.DTOs
{
    public class AddItemDto
    {
        public required string ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}