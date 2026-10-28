using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Core.EF.Application;
using VoucherService.Entities;
using VoucherService.Persistence;

namespace VoucherService.Services.Voucher;

public class ApplyVoucherHandler : IRequestHandler<ApplyVoucherCommand, AppResult<ApplyVoucherResult>>
{
    private readonly IVoucherUnitOfWork _unitOfWork;
    private readonly ILogger<ApplyVoucherHandler> _logger;

    public ApplyVoucherHandler(IVoucherUnitOfWork unitOfWork, ILogger<ApplyVoucherHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AppResult<ApplyVoucherResult>> Handle(ApplyVoucherCommand request, CancellationToken cancellationToken)
    {
        // Idempotency guard: if this OrderId was already applied before (e.g. gRPC client retry
        // on a transient network error), return the same result instead of double-applying.
        var existingApplied = await _unitOfWork.AppliedVoucherRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingApplied != null)
        {
            return AppResult<ApplyVoucherResult>.Success(new ApplyVoucherResult
            {
                VoucherId = existingApplied.VoucherId,
                VoucherCode = existingApplied.VoucherCode,
                DiscountAmount = existingApplied.DiscountAmount
            });
        }

        // Step 1: Load voucher by code, must be active
        var voucher = await _unitOfWork.VoucherRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (voucher == null || !voucher.IsActive)
            return AppResult<ApplyVoucherResult>.Failure("Voucher not found or inactive", 404);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Step 2: Check MaxUsagePerUser
            if (voucher.MaxUsagePerUser.HasValue)
            {
                var usedByUser = await _unitOfWork.AppliedVoucherRepository
                    .CountActiveByVoucherAndUserAsync(voucher.Id, request.UserId, cancellationToken);

                if (usedByUser >= voucher.MaxUsagePerUser.Value)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return AppResult<ApplyVoucherResult>.Failure("Bạn đã dùng hết lượt cho voucher này", 400);
                }
            }

            // Step 3: Atomic UsedCount UPDATE enforcing MaxUsageCount (no read-then-compare-then-update, see plan mục H.1)
            if (voucher.MaxUsageCount.HasValue)
            {
                var rowsAffected = await _unitOfWork.VoucherRepository
                    .TryIncrementUsedCountAsync(voucher.Id, voucher.MaxUsageCount.Value, cancellationToken);

                if (rowsAffected == 0)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return AppResult<ApplyVoucherResult>.Failure("Voucher đã hết lượt sử dụng", 400);
                }
            }

            // Step 4: Calculate discount using the shared pure calculator (same as preview)
            var calcItems = request.Items;
            var discountAmount = VoucherDiscountCalculator.Calculate(
                voucher.Scope,
                voucher.Type,
                voucher.Value,
                voucher.MaxDiscountAmount,
                request.SubTotal,
                request.ShippingCost,
                calcItems,
                voucher.ApplicableProductIds);

            // Step 5: Insert AppliedVoucher
            var applied = new AppliedVoucher
            {
                VoucherId = voucher.Id,
                VoucherCode = voucher.Code,
                OrderId = request.OrderId,
                UserId = request.UserId,
                Scope = voucher.Scope,
                Type = voucher.Type,
                DiscountAmount = discountAmount,
                OrderSubTotalAtApply = request.SubTotal,
                OrderShippingCostAtApply = request.ShippingCost,
                Status = AppliedVoucher.AppliedVoucherStatus.Applied,
                AppliedAt = DateTime.UtcNow
            };

            await _unitOfWork.AppliedVoucherRepository.AddAsync(applied, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return AppResult<ApplyVoucherResult>.Success(new ApplyVoucherResult
            {
                VoucherId = voucher.Id,
                VoucherCode = voucher.Code,
                DiscountAmount = discountAmount
            });
        }
        catch (Exception ex) when (IsUniqueOrderIdViolation(ex))
        {
            // Concurrent duplicate insert for the same OrderId (double-click / retry racing with itself).
            // Transaction was already rolled back inside CommitTransactionAsync's own catch block.
            _logger.LogWarning(ex, "Duplicate AppliedVoucher insert for OrderId {OrderId}, returning idempotent result", request.OrderId);

            var existing = await _unitOfWork.AppliedVoucherRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
            if (existing != null)
            {
                return AppResult<ApplyVoucherResult>.Success(new ApplyVoucherResult
                {
                    VoucherId = existing.VoucherId,
                    VoucherCode = existing.VoucherCode,
                    DiscountAmount = existing.DiscountAmount
                });
            }

            return AppResult<ApplyVoucherResult>.Failure("Failed to apply voucher", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying voucher {Code} for OrderId {OrderId}", request.Code, request.OrderId);
            await _unitOfWork.RollbackAsync(cancellationToken);
            return AppResult<ApplyVoucherResult>.Failure("Error applying voucher", 500);
        }
    }

    private static bool IsUniqueOrderIdViolation(Exception ex)
    {
        return ex is DbUpdateException { InnerException: PostgresException { SqlState: "23505" } };
    }
}
