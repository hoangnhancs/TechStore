using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.UnitOfWork;
using PaymentService.Data;
using PaymentService.Repositories.Interface;

namespace PaymentService.Persistence
{
    public class PaymentUnitOfWork : UnitOfWork<PaymentSvcDbContext>, IPaymentUnitOfWork
    {
        public IPaymentRepository PaymentRepository { get;}
        public PaymentUnitOfWork(
            PaymentSvcDbContext dbContext, 
            IPaymentRepository paymentRepository) : base(dbContext)
        {
            PaymentRepository = paymentRepository;
        }
    }
}