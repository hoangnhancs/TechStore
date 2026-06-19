using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using OrderService.DTOs;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order
{
    public class GetListOrdersInRangeDateQuery : IRequest<AppResult<List<OrderWithUserInforDto>>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}