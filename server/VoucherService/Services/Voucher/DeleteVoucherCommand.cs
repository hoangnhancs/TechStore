using MediatR;
using Shared.Core.EF.Application;

namespace VoucherService.Services.Voucher;

public class DeleteVoucherCommand : IRequest<AppResult<bool>>
{
    public required string Id { get; set; }
}
