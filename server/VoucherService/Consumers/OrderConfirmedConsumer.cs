using Contract.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using VoucherService.Data;
using static VoucherService.Entities.AppliedVoucher;

namespace VoucherService.Consumers;

/// <summary>
/// Async hook (plan mục C, "Sau khi order đã tạo thành công"). UsedCount was already reserved
/// at Apply time — Confirm does NOT touch UsedCount, it only flips the audit Status.
/// Idempotent: only acts when current Status is Applied (MassTransit outbox/redelivery may
/// call this more than once).
/// </summary>
public class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
{
    private readonly VoucherSvcDbContext _context;
    private readonly ILogger<OrderConfirmedConsumer> _logger;

    public OrderConfirmedConsumer(VoucherSvcDbContext context, ILogger<OrderConfirmedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var message = context.Message;

        var applied = await _context.AppliedVouchers
            .FirstOrDefaultAsync(a => a.OrderId == message.OrderId, context.CancellationToken);

        if (applied == null)
        {
            // No voucher was applied to this order — nothing to do.
            return;
        }

        if (applied.Status != AppliedVoucherStatus.Applied)
        {
            _logger.LogInformation("OrderConfirmed ignored for OrderId {OrderId}, AppliedVoucher Status already {Status}", message.OrderId, applied.Status);
            return;
        }

        applied.Status = AppliedVoucherStatus.Confirmed;
        applied.FinalizedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("AppliedVoucher confirmed for OrderId {OrderId}", message.OrderId);
    }
}
