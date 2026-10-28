using MediatR;
using Shared.Core.EF.Application;
using VoucherService.DTOs;

namespace VoucherService.Services.Voucher;

/// <summary>
/// Preview endpoint — computes discount with VoucherDiscountCalculator only, does NOT write
/// AppliedVoucher and does NOT check MaxUsageCount/MaxUsagePerUser (that enforcement only
/// happens in the real write flow, ApplyVoucherCommand — plan mục C "Endpoint preview").
/// </summary>
public class ValidateVoucherQuery : IRequest<AppResult<ValidateVoucherResultDto>>
{
    public required ValidateVoucherDto Dto { get; set; }
}
