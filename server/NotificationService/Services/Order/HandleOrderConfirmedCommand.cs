using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract.Order;
using MediatR;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Order
{
    public class HandleOrderConfirmedCommand : IRequest<AppResult<Unit>>
    {
        public required OrderConfirmed Message { get; set; }
    }
}