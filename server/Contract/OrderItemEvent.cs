using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contract
{
    public class OrderItemEvent
    {
        public required string ProductId { get; set; }
        public required int Quantity { get; set; }
        public required long UnitPrice { get; set; }
        public required string ProductName { get; set; }
        public required string ProductImageUrl { get; set; }
    }
}