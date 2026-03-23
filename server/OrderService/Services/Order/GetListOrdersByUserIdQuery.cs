using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using OrderService.DTOs;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order
{
    public class GetListOrdersByUserIdQuery : IRequest<AppResult<List<OrderDto>>>
    {
        public required string UserId { get; set; }
    }
}