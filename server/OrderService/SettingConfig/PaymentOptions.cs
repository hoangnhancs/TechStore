using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.SettingConfig
{
    public class PaymentOptions
    {
        public int OnlinePaymentWindowMinutes { get; set; }
        public int CodPaymentWindowMinutes { get; set; }
        public int MaxPaymentRetries { get; set; }
    }
}