using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract.Order;
using Microsoft.AspNetCore.SignalR;
using OrderService.SignalR;

namespace OrderService.Consumers
{
    public class OrderNotificationConsumer
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderNotificationConsumer(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Consume(OrderNotification notification)
        {
            // Send a SignalR message to the specific user about their order status
            await _hubContext.Clients
                .Group(notification.OrderId)
                .SendAsync("ReceiveOrderNotification", new
                {
                    OrderId = notification.OrderId,
                    IsSuccess = notification.IsSuccess,
                    ErrorMessage = notification.ErrorMessage
                });
        }
    }
}