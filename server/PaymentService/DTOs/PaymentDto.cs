using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.DTOs
{
    public class PaymentDto
    {
        public string? Id { get; set; }
        public string UserId { get; set; } = null!;
        public string OrderId { get; set; } = null!;
        public long Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
        public string Status { get; set; } = null!;
    }
}