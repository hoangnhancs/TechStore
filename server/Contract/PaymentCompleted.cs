using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contract
{
    public class PaymentCompleted
    {
        public required string OrderId { get; set; }
    }
}