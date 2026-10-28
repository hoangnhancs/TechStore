using MediatR;
using Shared.Core.EF.Application;

namespace VoucherService.Services.Voucher;

/// <summary>
/// Compensation / cancel flow (plan mục C bước 6). Must both:
/// 1. Flip AppliedVoucher.Status -> Released
/// 2. If the voucher has a MaxUsageCount, give back the reserved slot (UsedCount - 1)
/// Idempotent: if AppliedVoucher is not found, or already Confirmed/Released, this is a no-op success.
/// </summary>
public class ReleaseVoucherCommand : IRequest<AppResult<bool>>
{
    public required string OrderId { get; set; }
}
