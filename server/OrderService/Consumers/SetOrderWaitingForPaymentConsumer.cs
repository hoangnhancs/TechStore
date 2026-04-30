using Contract;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.Consumers;

/// <summary>
/// Sets order status to WaitingForPayment — triggered for COD orders after stock is reserved.
/// The order stays here until admin/user confirms via ConfirmCodOrder.
/// </summary>
public class SetOrderWaitingForPaymentConsumer : IConsumer<SetOrderWaitingForPayment>
{
    private readonly OrderSvcDbContext _context;
    private readonly ILogger<SetOrderWaitingForPaymentConsumer> _logger;

    public SetOrderWaitingForPaymentConsumer(OrderSvcDbContext context, ILogger<SetOrderWaitingForPaymentConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SetOrderWaitingForPayment> context)
    {
        var message = context.Message;
        _logger.LogInformation("SetOrderWaitingForPayment for OrderId: {OrderId}", message.OrderId);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == message.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderId}", message.OrderId);
            return;
        }

        order.WaitForPayment();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} is now WaitingForPayment", message.OrderId);
    }
}
