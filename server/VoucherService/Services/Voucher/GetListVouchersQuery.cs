using MediatR;
using Shared.Core.EF.Application;
using VoucherService.DTOs;

namespace VoucherService.Services.Voucher;

public class GetListVouchersQuery : IRequest<AppResult<PagedResult<VoucherDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
