using Contract.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using VoucherService.Data;
using static VoucherService.Entities.AppliedVoucher;

namespace VoucherService.Consumers;

/// <summary>
/// Async hook (plan mục C, "Sau khi order đã tạo thành công") for the case the Saga cancels an
/// already-created order (out of stock, payment expired...), as opposed to the synchronous
/// compensation in ReleaseVoucherCommand (cancelled during order creation itself).
/// Must flip Status -> Released AND give back the reserved UsedCount slot (same logic as
/// ReleaseVoucherCommand — plan mục C bước 6). Idempotent: only acts when Status is Applied.
/// </summary>
public class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly VoucherSvcDbContext _context;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(VoucherSvcDbContext context, ILogger<OrderCancelledConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelled> context)
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
            _logger.LogInformation("OrderCancelled ignored for OrderId {OrderId}, AppliedVoucher Status already {Status}", message.OrderId, applied.Status);
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(context.CancellationToken);
        try
        {
            applied.Status = AppliedVoucherStatus.Released;
            applied.FinalizedAt = DateTime.UtcNow;

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == applied.VoucherId, context.CancellationToken);
            if (voucher != null && voucher.MaxUsageCount.HasValue)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($"""
                    UPDATE "Vouchers"
                    SET "UsedCount" = "UsedCount" - 1
                    WHERE "Id" = {voucher.Id}
                    AND "UsedCount" > 0
                """, context.CancellationToken);
            }

            await _context.SaveChangesAsync(context.CancellationToken);
            await transaction.CommitAsync(context.CancellationToken);

            _logger.LogInformation("AppliedVoucher released for OrderId {OrderId} (order cancelled by Saga)", message.OrderId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(context.CancellationToken);
            _logger.LogError(ex, "Error releasing voucher for cancelled OrderId {OrderId}", message.OrderId);
            throw; // Let MassTransit handle retry
        }
    }
}
