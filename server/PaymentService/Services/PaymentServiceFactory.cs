using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Services.Interface;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services
{
    public class PaymentServiceFactory : IPaymentServiceFactory
    {
        private readonly IEnumerable<IPaymentService> _serviceStrategies;
        public PaymentServiceFactory(IEnumerable<IPaymentService> serviceStrategies)
        {
            _serviceStrategies = serviceStrategies;
        }
        public IPaymentService GetPaymentService(PaymentMethodType paymentMethod)
        {
            var service = _serviceStrategies.FirstOrDefault(s => s.Type == paymentMethod) ?? throw new NotSupportedException($"Payment method {paymentMethod.ToString()} is not supported.");
            return service;
        }
    }
}