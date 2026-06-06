using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contract.Payment
{
    public class PaymentCompleted
    {
        public required string OrderId { get; set; }
    }
}