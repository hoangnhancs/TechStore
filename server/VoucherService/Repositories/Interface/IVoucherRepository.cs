using Shared.Core.EF.Domain.Repositories;
using VoucherService.Entities;

namespace VoucherService.Repositories.Interface
{
    public interface IVoucherRepository : IBaseEFRepository<Voucher, string>
    {
        Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Voucher>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<int> CountAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Atomically increments UsedCount by 1 only if it is still below maxUsageCount.
        /// Returns the number of rows affected (0 = limit already reached, no update happened).
        /// Prevents TOCTOU race conditions (see plan mục H.1): never read-then-compare-then-update separately.
        /// </summary>
        Task<int> TryIncrementUsedCountAsync(string voucherId, int maxUsageCount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atomically decrements UsedCount by 1 (floor at 0), used when releasing a reserved slot.
        /// </summary>
        Task<int> DecrementUsedCountAsync(string voucherId, CancellationToken cancellationToken = default);
    }
}
