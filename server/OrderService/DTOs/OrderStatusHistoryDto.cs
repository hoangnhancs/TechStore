using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OrderService.Entities.Order;

namespace OrderService.DTOs
{
    public class OrderStatusHistoryDto
    {
        public required string OrderId { get; set; }
        public string? FromStatus { get; set; }
        public string? ToStatus { get; set; }
        public string? Note { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}