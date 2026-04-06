using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using EmailService.Interfaces;
using EmailService.Services.Interface;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Entities;

namespace OrderService.Consumers
{
    /// <summary>
    /// Consumer for ConfirmOrder command from OrderSaga
    /// Updates order status to Processing when stock is reserved successfully
    /// </summary>
    public class ConfirmOrderConsumer : IConsumer<ConfirmOrder>
    {
        private readonly OrderSvcDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateBuilder _templateBuilder;
        private readonly ILogger<ConfirmOrderConsumer> _logger;

        public ConfirmOrderConsumer(OrderSvcDbContext context, IEmailService emailService, IEmailTemplateBuilder templateBuilder, ILogger<ConfirmOrderConsumer> logger)
        {
            _context = context;
            _emailService = emailService;
            _templateBuilder = templateBuilder;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ConfirmOrder> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing ConfirmOrder for OrderId: {OrderId}", message.OrderId);

            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == message.OrderId);

                if (order == null)
                {
                    _logger.LogWarning("Order not found: {OrderId}", message.OrderId);
                    return;
                }

                // Update order status to Processing
                order.Process();

                var body = await _templateBuilder.BuildAsync("OrderConfirmation", new()
                {
                    ["OrderId"] = order.Id.ToString(),
                    ["CustomerName"] = order.UserId.ToString(), // Ideally should fetch user details for name
                    ["TotalPrice"] = order.Total.ToString("C"),
                    ["Address"] = order.BillingAddress ?? "N/A",
                    ["OrderDate"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                });

                await _emailService.SendEmailAsync("thaihoangnhantk17lqd@gmail.com", "Xác nhận đơn hàng", body);
                
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
