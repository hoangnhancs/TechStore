using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using PaymentService.DTOs;
using Shared.Core.EF.Application;

namespace PaymentService.Services.Payment
{
    public class CreatePaymentCommand : IRequest<AppResult<PaymentDto>>
    {
        public required CreatePaymentDto CreatePaymentDto { get; set; }
    }
}