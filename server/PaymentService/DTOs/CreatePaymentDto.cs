using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.DTOs
{
    public class CreatePaymentDto
    {
        public required string UserId { get; set; }
        public required string OrderId { get; set; }
        public required long Amount { get; set; }
        public required string Currency { get; set; }
        public required string PaymentMethod { get; set; }
    }
}