using Infrastructure.EF.UnitOfWork;
using VoucherService.Data;
using VoucherService.Repositories;
using VoucherService.Repositories.Interface;

namespace VoucherService.Persistence
{
    public class VoucherUnitOfWork : UnitOfWork<VoucherSvcDbContext>, IVoucherUnitOfWork
    {
        private IVoucherRepository? _voucherRepository;
        private IAppliedVoucherRepository? _appliedVoucherRepository;

        public IVoucherRepository VoucherRepository =>
            _voucherRepository ??= new VoucherRepository(_dbContext);

        public IAppliedVoucherRepository AppliedVoucherRepository =>
            _appliedVoucherRepository ??= new AppliedVoucherRepository(_dbContext);

        public VoucherUnitOfWork(VoucherSvcDbContext context) : base(context)
        {
        }
    }
}
