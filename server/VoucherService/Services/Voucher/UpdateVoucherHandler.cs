using AutoMapper;
using MediatR;
using Shared.Core.EF.Application;
using VoucherService.DTOs;
using VoucherService.Persistence;

namespace VoucherService.Services.Voucher;

public class UpdateVoucherHandler : IRequestHandler<UpdateVoucherCommand, AppResult<VoucherDto>>
{
    private readonly IVoucherUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateVoucherHandler(IVoucherUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppResult<VoucherDto>> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var voucher = await _unitOfWork.VoucherRepository.GetByIdAsync(request.Id, cancellationToken);
        if (voucher == null || voucher.IsDeleted)
            return AppResult<VoucherDto>.Failure("Voucher not found", 404);

        if (!Enum.TryParse<Entities.Voucher.DiscountScope>(dto.Scope, ignoreCase: true, out var scope))
            return AppResult<VoucherDto>.Failure($"Invalid Scope '{dto.Scope}'", 400);

        if (!Enum.TryParse<Entities.Voucher.DiscountType>(dto.Type, ignoreCase: true, out var type))
            return AppResult<VoucherDto>.Failure($"Invalid Type '{dto.Type}'", 400);

        voucher.Name = dto.Name;
        voucher.Description = dto.Description;
        voucher.Scope = scope;
        voucher.Type = type;
        voucher.Value = dto.Value;
        voucher.MaxDiscountAmount = dto.MaxDiscountAmount;
        voucher.MinOrderAmount = dto.MinOrderAmount;
        voucher.MaxUsageCount = dto.MaxUsageCount;
        voucher.MaxUsagePerUser = dto.MaxUsagePerUser;
        voucher.StartAt = dto.StartAt;
        voucher.ExpiredAt = dto.ExpiredAt;
        voucher.IsActive = dto.IsActive;
        voucher.ApplicableProductIds = dto.ApplicableProductIds;

        _unitOfWork.VoucherRepository.Update(voucher);
        var result = await _unitOfWork.CommitAsync(cancellationToken);
        if (!result)
            return AppResult<VoucherDto>.Failure("Failed to update voucher", 400);

        return AppResult<VoucherDto>.Success(_mapper.Map<VoucherDto>(voucher));
    }
}
