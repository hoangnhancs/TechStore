using MediatR;
using Shared.Core.EF.Application;
using VoucherService.DTOs;

namespace VoucherService.Services.Voucher;

public class CreateVoucherCommand : IRequest<AppResult<VoucherDto>>
{
    public required CreateVoucherDto Dto { get; set; }
}
