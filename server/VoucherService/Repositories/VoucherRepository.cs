using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using VoucherService.Data;
using VoucherService.Entities;
using VoucherService.Repositories.Interface;

namespace VoucherService.Repositories
{
    public class VoucherRepository : BaseEFRepository<Voucher, string, VoucherSvcDbContext>, IVoucherRepository
    {
        public VoucherRepository(VoucherSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var normalized = code.Trim().ToUpperInvariant();
            return await DbSet
                .Where(v => v.IsDeleted == false && v.Code == normalized)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Voucher>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(v => v.IsDeleted == false)
                .OrderByDescending(v => v.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(v => v.IsDeleted == false).CountAsync(cancellationToken);
        }

        public async Task<int> TryIncrementUsedCountAsync(string voucherId, int maxUsageCount, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE "Vouchers"
                SET "UsedCount" = "UsedCount" + 1
                WHERE "Id" = {voucherId}
                AND "UsedCount" < {maxUsageCount}
            """, cancellationToken);
        }

        public async Task<int> DecrementUsedCountAsync(string voucherId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE "Vouchers"
                SET "UsedCount" = "UsedCount" - 1
                WHERE "Id" = {voucherId}
                AND "UsedCount" > 0
            """, cancellationToken);
        }
    }
}
