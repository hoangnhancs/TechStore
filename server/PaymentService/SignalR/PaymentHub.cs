using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PaymentService.SignalR
{
    [AllowAnonymous]
    public class PaymentHub : Hub
    {
        private readonly IMediator _mediator;
        public PaymentHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task GetPayment(string orderId)
        {
            // This method can be used to fetch current payment status for a given order
            // Implementation can be added as needed, e.g., querying payment status from database
            await Clients.Caller.SendAsync("ReceivePaymentStatus", $"Payment status for OrderId: {orderId} is currently unavailable.");
        }

        public async Task JoinPaymentGroup(string orderId)
        {
            //var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (string.IsNullOrEmpty(userId))
            //{
            //    await Clients.Caller.SendAsync("JoinGroupError", "User not authenticated");
            //    return;
            //}
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
        }

        public async Task LeavePaymentGroup(string orderId)
        {
            //var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (string.IsNullOrEmpty(userId))
            //{
            //    await Clients.Caller.SendAsync("LeaveGroupError", "User not authenticated");
            //    return;
            //}
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderId);
        }

    }
}