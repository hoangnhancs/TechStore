using Contract;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.Consumers;

/// <summary>
/// Confirms a COD order (moves to Processing) when admin/user approves on FE.
/// The Saga sends ConfirmOrder after this, which triggers ConfirmOrderConsumer.
/// </summary>
public class ConfirmCodOrderConsumer : IConsumer<ConfirmCodOrder>
{
    private readonly OrderSvcDbContext _context;
    private readonly ILogger<ConfirmCodOrderConsumer> _logger;

    public ConfirmCodOrderConsumer(OrderSvcDbContext context, ILogger<ConfirmCodOrderConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ConfirmCodOrder> context)
    {
        //ignore consume change state to processing when publish ConfirmCodOrder, istatus will be changed after create payment rơ successfully, and publish ConfirmOrder, then change state to processing in ConfirmOrderConsumer

        
        // var message = context.Message;
        // _logger.LogInformation("ConfirmCodOrder for OrderId: {OrderId}", message.OrderId);

        // var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == message.OrderId);
        // if (order == null)
        // {
        //     _logger.LogWarning("Order not found: {OrderId}", message.OrderId);
        //     return;
        // }

        // order.ManualProcess();
        // await _context.SaveChangesAsync();

        // _logger.LogInformation("COD Order {OrderId} manually confirmed and set to Processing", message.OrderId);
    }
}
