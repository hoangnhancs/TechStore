using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.DTOs
{
    public class PaymentIntentDto
    {
        public required string Id { get; set; }
        public required string ClientSecret { get; set; }
    }
}