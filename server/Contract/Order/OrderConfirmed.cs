using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Order
{
    public class OrderConfirmed
    {
        public required string OrderId { get; set; }
        public required string OrderNo { get; set; }
        public required DateTime CreatedDate { get; set; }
        public required string UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string? UserImageUrl { get; set; }
        public List<OrderItemEvent> Items { get; set; } = [];
        public decimal SubTotal { get; set; }
        public required string Address { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
    }
}
