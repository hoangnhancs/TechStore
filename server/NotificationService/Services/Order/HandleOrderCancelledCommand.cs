using Contract.Order;
using MediatR;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Order
{
    public class HandleOrderCancelledCommand : IRequest<AppResult<Unit>>
    {
        public required OrderCancelled Message { get; set; }
    }
}
