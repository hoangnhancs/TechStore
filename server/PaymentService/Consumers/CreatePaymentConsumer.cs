using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Persistence;
using PaymentService.Services.Interface;
using PaymentService.SignalR;
using static PaymentService.Entities.Payment;

namespace PaymentService.Consumers
{
    public class CreatePaymentConsumer : IConsumer<CreatePayment>
    {
        private readonly IHubContext<PaymentHub> _hubContext;
        private readonly IPaymentServiceFactory _paymentServiceFactory;
        private readonly ILogger<CreatePaymentConsumer> _logger;
        public CreatePaymentConsumer(
            IHubContext<PaymentHub> hubContext,
            IPaymentServiceFactory paymentServiceFactory,
            ILogger<CreatePaymentConsumer> logger)
        {
            _hubContext = hubContext;
            _paymentServiceFactory = paymentServiceFactory;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<CreatePayment> context)
        {
            var message = context.Message;
            
            if (!Enum.TryParse<PaymentMethodType>(message.PaymentMethod, out var paymentMethod))
            {
                _logger.LogError("Invalid payment method: {PaymentMethod} for OrderId: {OrderId}", message.PaymentMethod, message.OrderId);
                await context.Publish(new PaymentFailed
                {
                    OrderId = message.OrderId,
                    ErrorMessage = $"Invalid payment method: {message.PaymentMethod}"
                });
                return; // Skip processing if payment method is invalid
            }
            try
            {
                var service = _paymentServiceFactory.GetPaymentService(paymentMethod);
                var payment = await service.CreatePayment(new CreatePaymentDto
                {
                    UserId = message.UserId,
                    OrderId = message.OrderId,
                    Amount = message.Amount,
                    Currency = message.Currency,
                    PaymentMethod = message.PaymentMethod
                });

                await _hubContext.Clients
                    .Group(message.OrderId)
                    .SendAsync("ReceivePayment", payment, context.CancellationToken);

                _logger.LogInformation("Payment created successfully for OrderId: {OrderId}, waiting for user to complete payment", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for OrderId: {OrderId}", message.OrderId);
                await context.Publish(new PaymentFailed
                {
                    OrderId = message.OrderId,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}