using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contract.Payment
{
    /// <summary>
    /// Command to retry payment creation after it failed
    /// Used when PaymentService failed to create payment (e.g., COD after admin confirm)
    /// </summary>
    public class RetryCreatePayment
    {
        public required string OrderId { get; set; }
        public required string UserId { get; set; }
    }
}
