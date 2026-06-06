using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contract.Payment
{
    public class PaymentFailed
    {
        public required string OrderId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}