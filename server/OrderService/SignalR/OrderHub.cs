using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using OrderService.DTOs;
using OrderService.Entities;
using OrderService.Services.OrderHistory;

namespace OrderService.SignalR
{
    public class OrderHub : Hub
    {
        private readonly IMediator _mediator;
        public OrderHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task LoadOrderHistory(string orderId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier); // Assuming UserIdentifier is set to the user's ID
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("OrderHistoryError", "User not authenticated");
                return;
            }
            var res = await _mediator.Send(new GetOrderStatusHistoryCommand { UserId = userId, OrderId = orderId });
            if (res.IsSuccess && res.Value != null)          
            {

                await Clients.Caller.SendAsync("ReceiveOrderHistory", res.Value);   
            }
            else if (res.IsSuccess && res.Value == null)
            {
                await Clients.Group(userId).SendAsync("ReceiveOrderHistory", new OrderStatusHistoryWithShipmentDto());
            }
            else
            {
                await Clients.Caller.SendAsync("OrderHistoryError", res.Error ?? "Failed to load order history");
            }
        }
    }
}