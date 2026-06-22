using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Entities;

namespace OrderService.Consumers
{
    /// <summary>
    /// Consumer for CancelOrder command from OrderSaga
    /// Updates order status to Cancelled when reservation or other steps fail
    /// </summary>
    public class CancelOrderConsumer : IConsumer<CancelOrder>
    {
        private readonly OrderSvcDbContext _context;
        private readonly ILogger<CancelOrderConsumer> _logger;

        public CancelOrderConsumer(OrderSvcDbContext context, ILogger<CancelOrderConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CancelOrder> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing CancelOrder for OrderId: {OrderId}, Reason: {Reason}", 
                message.OrderId, message.Reason);

            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == message.OrderId);

                if (order == null)
                {
                    _logger.LogWarning("Order not found: {OrderId}", message.OrderId);
                    return;
                }

                // Update order status to Cancelled
                order.Cancel(context.Message.Reason);

                _logger.LogInformation("Order cancelled: {OrderId}, Reason: {Reason}",
                    message.OrderId, message.Reason);

                await context.Publish<Contract.Order.OrderCancelled>(new
                {
                    OrderId = order.Id,
                    OrderNo = order.OrderNo,
                    UserId = order.UserId,
                    Reason = message.Reason,
                    PaymentMethod = order.PmtMethod.ToString(),
                    CancelledAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order: {OrderId}", message.OrderId);
                throw; // Let MassTransit handle retry
            }
        }
    }
}
