using Shared.Core.EF.UnitOfWork;
using VoucherService.Repositories.Interface;

namespace VoucherService.Persistence
{
    public interface IVoucherUnitOfWork : IUnitOfWork
    {
        IVoucherRepository VoucherRepository { get; }
        IAppliedVoucherRepository AppliedVoucherRepository { get; }
    }
}
