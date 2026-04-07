using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.DTOs;
using PaymentService.Services.Interface;

namespace PaymentService.Services
{
    public class VNPayPaymentService : IWebhookPaymentService
    {
        public Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            throw new NotImplementedException();
        }

        public Task<PaymentDto> HandleWebhook(HttpRequest request)
        {
            throw new NotImplementedException();
        }
    }
}