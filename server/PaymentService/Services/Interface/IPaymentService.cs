using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.DTOs;

namespace PaymentService.Services.Interface
{
    public interface IPaymentService
    {
        Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto);
    }
}