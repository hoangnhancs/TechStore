using MediatR;
using Shared.Core.EF.Application;
using VoucherService.Persistence;
using static VoucherService.Entities.AppliedVoucher;

namespace VoucherService.Services.Voucher;

public class ReleaseVoucherHandler : IRequestHandler<ReleaseVoucherCommand, AppResult<bool>>
{
    private readonly IVoucherUnitOfWork _unitOfWork;
    private readonly ILogger<ReleaseVoucherHandler> _logger;

    public ReleaseVoucherHandler(IVoucherUnitOfWork unitOfWork, ILogger<ReleaseVoucherHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AppResult<bool>> Handle(ReleaseVoucherCommand request, CancellationToken cancellationToken)
    {
        var applied = await _unitOfWork.AppliedVoucherRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (applied == null)
        {
            // Nothing was applied for this OrderId — idempotent no-op success.
            return AppResult<bool>.Success(true);
        }

        if (applied.Status != AppliedVoucherStatus.Applied)
        {
            // Already Confirmed or Released by a previous call (idempotent guard, MassTransit redelivery).
            _logger.LogInformation("ReleaseVoucher skipped for OrderId {OrderId}, current Status is {Status}", request.OrderId, applied.Status);
            return AppResult<bool>.Success(true);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            applied.Status = AppliedVoucherStatus.Released;
            applied.FinalizedAt = DateTime.UtcNow;
            _unitOfWork.AppliedVoucherRepository.Update(applied);

            var voucher = await _unitOfWork.VoucherRepository.GetByIdAsync(applied.VoucherId, cancellationToken);
            if (voucher != null && voucher.MaxUsageCount.HasValue)
            {
                await _unitOfWork.VoucherRepository.DecrementUsedCountAsync(voucher.Id, cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return AppResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing voucher for OrderId {OrderId}", request.OrderId);
            await _unitOfWork.RollbackAsync(cancellationToken);
            return AppResult<bool>.Failure("Error releasing voucher", 500);
        }
    }
}
