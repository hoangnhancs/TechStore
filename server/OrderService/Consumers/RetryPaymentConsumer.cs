using Contract.Payment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using static OrderService.Entities.Order;

namespace OrderService.Consumers;

/// <summary>
/// Resets order payment state so a new payment attempt can be made.
/// The Saga publishes CreatePayment after receiving this, so payment service handles the actual charge.
/// </summary>
public class RetryPaymentConsumer : IConsumer<RetryPayment>
{
    private readonly OrderSvcDbContext _context;
    private readonly ILogger<RetryPaymentConsumer> _logger;

    public RetryPaymentConsumer(OrderSvcDbContext context, ILogger<RetryPaymentConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RetryPayment> context)
    {
        var message = context.Message;
        _logger.LogInformation("RetryPayment for OrderId: {OrderId}, NewMethod: {Method}", message.OrderId, message.PaymentMethod);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == message.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderId}", message.OrderId);
            return;
        }

        if (Enum.TryParse<PaymentMethod>(message.PaymentMethod, out var method))
        {
            order.UpdatePaymentMethod(method);
        }

        order.RetryPayment();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} payment reset for retry", message.OrderId);
    }
}
