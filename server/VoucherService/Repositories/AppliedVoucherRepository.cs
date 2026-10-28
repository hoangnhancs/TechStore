using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using VoucherService.Data;
using VoucherService.Entities;
using VoucherService.Repositories.Interface;
using static VoucherService.Entities.AppliedVoucher;

namespace VoucherService.Repositories
{
    public class AppliedVoucherRepository : BaseEFRepository<AppliedVoucher, string, VoucherSvcDbContext>, IAppliedVoucherRepository
    {
        public AppliedVoucherRepository(VoucherSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<AppliedVoucher?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
        {
            return await DbSet.FirstOrDefaultAsync(a => a.OrderId == orderId, cancellationToken);
        }

        public async Task<int> CountActiveByVoucherAndUserAsync(string voucherId, string userId, CancellationToken cancellationToken = default)
        {
            return await DbSet.CountAsync(a =>
                a.VoucherId == voucherId &&
                a.UserId == userId &&
                (a.Status == AppliedVoucherStatus.Applied || a.Status == AppliedVoucherStatus.Confirmed),
                cancellationToken);
        }
    }
}
