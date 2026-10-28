using MediatR;
using Shared.Core.EF.Application;
using VoucherService.DTOs;
using VoucherService.Persistence;

namespace VoucherService.Services.Voucher;

public class ValidateVoucherHandler : IRequestHandler<ValidateVoucherQuery, AppResult<ValidateVoucherResultDto>>
{
    private readonly IVoucherUnitOfWork _unitOfWork;

    public ValidateVoucherHandler(IVoucherUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppResult<ValidateVoucherResultDto>> Handle(ValidateVoucherQuery request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var voucher = await _unitOfWork.VoucherRepository.GetByCodeAsync(dto.Code, cancellationToken);
        if (voucher == null || !voucher.IsActive)
            return AppResult<ValidateVoucherResultDto>.Failure("Voucher not found or inactive", 404);

        var calcItems = dto.Items?.Select(i => new VoucherDiscountCalculator.CalcItem(i.ProductId, i.Quantity, i.UnitPrice)).ToList();

        var discountAmount = VoucherDiscountCalculator.Calculate(
            voucher.Scope,
            voucher.Type,
            voucher.Value,
            voucher.MaxDiscountAmount,
            dto.SubTotal,
            dto.ShippingCost,
            calcItems,
            voucher.ApplicableProductIds);

        return AppResult<ValidateVoucherResultDto>.Success(new ValidateVoucherResultDto
        {
            Code = voucher.Code,
            Scope = voucher.Scope.ToString(),
            Type = voucher.Type.ToString(),
            DiscountAmount = discountAmount
        });
    }
}
