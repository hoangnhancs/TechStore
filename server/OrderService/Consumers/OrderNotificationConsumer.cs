using Contract.Order;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using OrderService.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Consumers
{
    public class OrderNotificationConsumer : IConsumer<OrderNotification>
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderNotificationConsumer(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<OrderNotification> context)
        {
            var message = context.Message;
            await _hubContext.Clients
                .Group(message.OrderId)
                .SendAsync("ReceiveOrderNotification", new
                {
                    OrderId = message.OrderId,
                    IsSuccess = message.IsSuccess,
                    ErrorMessage = message.ErrorMessage
                });
        }
    }
}