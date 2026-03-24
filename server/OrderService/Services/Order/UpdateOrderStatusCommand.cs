using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order
{
    public class UpdateOrderStatusCommand : IRequest<AppResult<Unit>>
    {
        public required string OrderId { get; set; }
        public required Entities.Order.OrderStatus NewStatus { get; set; }
    }
}