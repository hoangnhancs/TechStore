using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contract
{
    public class StockReserved
    {
        public required string OrderId { get; set; }
        public List<OrderItemEvent> Items { get; set; } = [];
    }
}