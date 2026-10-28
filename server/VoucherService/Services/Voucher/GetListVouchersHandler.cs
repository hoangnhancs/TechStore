using AutoMapper;
using MediatR;
using Shared.Core.EF.Application;
using VoucherService.DTOs;
using VoucherService.Persistence;

namespace VoucherService.Services.Voucher;

public class GetListVouchersHandler : IRequestHandler<GetListVouchersQuery, AppResult<PagedResult<VoucherDto>>>
{
    private readonly IVoucherUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetListVouchersHandler(IVoucherUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppResult<PagedResult<VoucherDto>>> Handle(GetListVouchersQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var vouchers = await _unitOfWork.VoucherRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        var totalCount = await _unitOfWork.VoucherRepository.CountAllAsync(cancellationToken);

        var dtos = _mapper.Map<List<VoucherDto>>(vouchers);

        return AppResult<PagedResult<VoucherDto>>.Success(new PagedResult<VoucherDto>(dtos, totalCount, pageNumber, pageSize));
    }
}
