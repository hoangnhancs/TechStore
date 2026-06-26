using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Entities;
using PaymentService.Repositories.Interface;

namespace PaymentService.Repositories
{
    public class PaymentRepository : BaseEFRepository<Payment, string, PaymentSvcDbContext>, IPaymentRepository
    {
        public PaymentRepository(PaymentSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Payment?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
        {
            return await DbSet.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
        }
    }
}
