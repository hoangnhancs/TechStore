using Contract.Order;
using MassTransit;
using MediatR;
using NotificationService.Services.Order;

namespace NotificationService.Consumers
{
    public class OrderCancelledConsumer : IConsumer<OrderCancelled>
    {
        private readonly IMediator _mediator;
        public OrderCancelledConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Consume(ConsumeContext<OrderCancelled> context)
        {
            await _mediator.Send(new HandleOrderCancelledCommand { Message = context.Message });
        }
    }
}
