using MediatR;
using Shared.Core.EF.Application;
using VoucherService.Persistence;

namespace VoucherService.Services.Voucher;

/// <summary>
/// Soft delete: MarkAsDeleted (BaseEntity) + IsActive = false, so Apply/Validate immediately
/// stop accepting the code without needing a separate "deactivate" endpoint.
/// </summary>
public class DeleteVoucherHandler : IRequestHandler<DeleteVoucherCommand, AppResult<bool>>
{
    private readonly IVoucherUnitOfWork _unitOfWork;

    public DeleteVoucherHandler(IVoucherUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppResult<bool>> Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _unitOfWork.VoucherRepository.GetByIdAsync(request.Id, cancellationToken);
        if (voucher == null || voucher.IsDeleted)
            return AppResult<bool>.Failure("Voucher not found", 404);

        voucher.IsActive = false;
        _unitOfWork.VoucherRepository.Delete(voucher); // MarkAsDeleted + Update

        var result = await _unitOfWork.CommitAsync(cancellationToken);
        if (!result)
            return AppResult<bool>.Failure("Failed to delete voucher", 400);

        return AppResult<bool>.Success(true);
    }
}
