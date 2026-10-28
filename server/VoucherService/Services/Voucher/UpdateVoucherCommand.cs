using MediatR;
using Shared.Core.EF.Application;
using VoucherService.DTOs;

namespace VoucherService.Services.Voucher;

public class UpdateVoucherCommand : IRequest<AppResult<VoucherDto>>
{
    public required string Id { get; set; }
    public required UpdateVoucherDto Dto { get; set; }
}
