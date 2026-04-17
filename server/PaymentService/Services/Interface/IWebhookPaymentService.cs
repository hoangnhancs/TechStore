using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.DTOs;

namespace PaymentService.Services.Interface
{
    public interface IWebhookPaymentService : IPaymentService
    {
        Task HandleWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken = default);
    }
}