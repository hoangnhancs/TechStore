using Contract.Order;
using MassTransit;
using MediatR;
using NotificationService.Services.Order;

namespace NotificationService.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly IMediator _mediator;
        public OrderCreatedConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            await _mediator.Send(new HandleOrderCreatedCommand
            {
                Message = context.Message
            });
        }
    }
}
