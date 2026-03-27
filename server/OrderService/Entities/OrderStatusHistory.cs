using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;
using static OrderService.Entities.Order;

namespace OrderService.Entities
{
    public class OrderStatusHistory : BaseEntity<int>
    {
        public required string OrderId { get; set; }
        public OrderStatus FromStatus { get; set; }
        public OrderStatus ToStatus { get; set; }
        public string? Note { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
    }
}