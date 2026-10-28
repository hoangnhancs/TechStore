using MediatR;
using Shared.Core.EF.Application;

namespace VoucherService.Services.Voucher;

/// <summary>
/// Write flow (plan mục C) — called from GrpcVoucherService, OrderService is the only caller.
/// Must run the 5 steps strictly in order inside one transaction:
/// 1. Load Voucher by Code (IsActive) 2. Check MaxUsagePerUser 3. Atomic UsedCount UPDATE
/// (enforces MaxUsageCount, never read-then-compare-then-update) 4. Calculate discount
/// 5. Insert AppliedVoucher.
/// </summary>
public class ApplyVoucherCommand : IRequest<AppResult<ApplyVoucherResult>>
{
    public required string Code { get; set; }
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
    public required long SubTotal { get; set; }
    public long ShippingCost { get; set; } = 0;
    public List<VoucherDiscountCalculator.CalcItem>? Items { get; set; }
}

public class ApplyVoucherResult
{
    public required string VoucherId { get; set; }
    public required string VoucherCode { get; set; }
    public required long DiscountAmount { get; set; }
}
