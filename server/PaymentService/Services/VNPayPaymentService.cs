using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.DTOs;
using PaymentService.Services.Interface;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services
{
    public class VNPayPaymentService : IWebhookPaymentService
    {
        public PaymentMethodType Type => PaymentMethodType.VNPay;

        public Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            throw new NotImplementedException();
        }

        public Task HandleWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}