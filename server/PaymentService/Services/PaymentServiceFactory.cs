using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.Services.Interface;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services
{
    public class PaymentServiceFactory : IPaymentServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public PaymentServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IPaymentService GetPaymentService(PaymentMethodType paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodType.CashOnDelivery => _serviceProvider.GetRequiredService<CODPaymentService>(),
                PaymentMethodType.CreditCard    => _serviceProvider.GetRequiredService<StripePaymentService>(),
                PaymentMethodType.Momo          => _serviceProvider.GetRequiredService<MomoPaymentService>(),
                PaymentMethodType.VNPay         => _serviceProvider.GetRequiredService<VNPayPaymentService>(),
                PaymentMethodType.BankTransfer  => _serviceProvider.GetRequiredService<BankTransferPaymentService>(),
                _ => throw new NotSupportedException($"Payment method {paymentMethod} not supported")
            };
        }
    }
}