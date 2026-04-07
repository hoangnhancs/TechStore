using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services.Interface
{
    public interface IPaymentServiceFactory
    {
        IPaymentService GetPaymentService(PaymentMethodType method);
    }
}