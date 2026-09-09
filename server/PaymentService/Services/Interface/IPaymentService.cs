using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.DTOs;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services.Interface
{
    public interface IPaymentService
    {
        PaymentMethodType Type { get; }
        Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto);
    }
}