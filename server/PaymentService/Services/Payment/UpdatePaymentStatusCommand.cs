using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Core.EF.Application;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services.Payment
{
    public class UpdatePaymentStatusCommand : IRequest<AppResult<Unit>>
    {
        public required string OrderId { get; set; }
        public PaymentStatus NewStatus { get; set; }
    }
}