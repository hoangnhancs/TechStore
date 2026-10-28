using AutoMapper;
using MediatR;
using Shared.Core.EF.Application;
using VoucherService.DTOs;
using VoucherService.Persistence;

namespace VoucherService.Services.Voucher;

public class GetVoucherByIdHandler : IRequestHandler<GetVoucherByIdQuery, AppResult<VoucherDto>>
{
    private readonly IVoucherUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetVoucherByIdHandler(IVoucherUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppResult<VoucherDto>> Handle(GetVoucherByIdQuery request, CancellationToken cancellationToken)
    {
        var voucher = await _unitOfWork.VoucherRepository.GetByIdAsync(request.Id, cancellationToken);
        if (voucher == null || voucher.IsDeleted)
            return AppResult<VoucherDto>.Failure("Voucher not found", 404);

        return AppResult<VoucherDto>.Success(_mapper.Map<VoucherDto>(voucher));
    }
}
