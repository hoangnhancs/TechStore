using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;
using static OrderService.Entities.Order;

namespace OrderService.Entities
{
    public class OrderStatusHistory : BaseEntity<int>
    {
        public required string OrderId { get; set; }
        [Column(TypeName = "varchar(20)")]
        public OrderStatus? FromStatus { get; set; }
        [Column(TypeName = "varchar(20)")]
        public OrderStatus ToStatus { get; set; }
        public string? Note { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
    }
}