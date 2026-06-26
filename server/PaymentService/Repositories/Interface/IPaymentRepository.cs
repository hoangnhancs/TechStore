using PaymentService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace PaymentService.Repositories.Interface
{
    public interface IPaymentRepository : IBaseEFRepository<Payment, string>
    {
        Task<Payment?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
    }
}
