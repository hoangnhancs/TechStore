using MediatR;
using OrderService.DTOs;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order
{
    public class GetListOrdersWaitingForConfirmationQuery : IRequest<AppResult<List<OrderWithUserInforDto>>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
