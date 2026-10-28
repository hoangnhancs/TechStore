using Shared.Core.EF.Domain.Repositories;
using VoucherService.Entities;

namespace VoucherService.Repositories.Interface
{
    public interface IAppliedVoucherRepository : IBaseEFRepository<AppliedVoucher, string>
    {
        Task<AppliedVoucher?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);

        Task<int> CountActiveByVoucherAndUserAsync(string voucherId, string userId, CancellationToken cancellationToken = default);
    }
}
