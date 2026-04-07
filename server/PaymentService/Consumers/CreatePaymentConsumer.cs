using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using MassTransit;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Persistence;
using PaymentService.Services.Interface;
using static PaymentService.Entities.Payment;

namespace PaymentService.Consumers
{
    public class CreatePaymentConsumer : IConsumer<CreatePayment>
    {
        private readonly IPaymentUnitOfWork _unitOfWork;
        private readonly IPaymentServiceFactory _paymentServiceFactory;
        private readonly ILogger<CreatePaymentConsumer> _logger;
        public CreatePaymentConsumer(IPaymentUnitOfWork unitOfWork, IPaymentServiceFactory paymentServiceFactory, ILogger<CreatePaymentConsumer> logger)
        {
            _unitOfWork = unitOfWork;
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
                await service.CreatePayment(new CreatePaymentDto
                {
                    UserId = message.UserId,
                    OrderId = message.OrderId,
                    Amount = message.Amount,
                    Currency = message.Currency,
                    PaymentMethod = message.PaymentMethod
                });
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