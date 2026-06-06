using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Contract.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Entities;
using static OrderService.Entities.Order;

namespace OrderService.Consumers
{
    /// <summary>
    /// Consumer for ConfirmOrder command from OrderSaga
    /// Updates order status to Processing when stock is reserved successfully
    /// </summary>
    public class ConfirmOrderConsumer : IConsumer<ConfirmOrder>
    {
        private readonly OrderSvcDbContext _context;
        private readonly ILogger<ConfirmOrderConsumer> _logger;

        public ConfirmOrderConsumer(OrderSvcDbContext context, ILogger<ConfirmOrderConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ConfirmOrder> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing ConfirmOrder for OrderId: {OrderId}", message.OrderId);

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == message.OrderId);

                if (order == null)
                {
                    _logger.LogWarning("Order not found: {OrderId}", message.OrderId);
                    return;
                }

                // Update order status to Processing
                if (order.PmtMethod == PaymentMethod.CashOnDelivery)
                {
                    order.ManualProcess(); // For COD, we can directly mark as Processing without waiting for payment confirmation
                }
                else
                {
                    order.Process();
                }

                await context.Publish(new OrderConfirmed
                {
                    OrderId = message.OrderId,
                    OrderNo = order.OrderNo,
                    CreatedDate = order.CreatedAt,
                    UserId = order.UserId,
                    UserName = order.RecipientName,
                    UserEmail = order.UserEmail,
                    UserPhone = order.RecipientPhone,
                    UserImageUrl = order.UserImageUrl,
                    Items = order.Items.Select(i => new OrderItemEvent
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName ?? "",
                        ProductImageUrl = i.ProductImageUrl ?? "",
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList(),
                    SubTotal = order.SubTotal,
                    Address = order.ShippingAddress ?? "",
                    ShippingCost = order.ShippingCost,
                    Discount = order.Discount,
                    Total = order.Total
                });

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order confirmed and marked as Processing: {OrderId}", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order: {OrderId}", message.OrderId);
                throw; // Let MassTransit handle retry
            }
        }
    }
}
