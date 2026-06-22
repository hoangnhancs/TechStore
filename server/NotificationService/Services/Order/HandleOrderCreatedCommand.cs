using Contract.Order;
using MediatR;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Order
{
    public class HandleOrderCreatedCommand : IRequest<AppResult<Unit>>
    {
        public required OrderCreated Message { get; set; }
    }
}
