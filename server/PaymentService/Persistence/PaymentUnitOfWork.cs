using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.UnitOfWork;
using PaymentService.Data;
using PaymentService.Repositories;
using PaymentService.Repositories.Interface;

namespace PaymentService.Persistence
{
    public class PaymentUnitOfWork : UnitOfWork<PaymentSvcDbContext>, IPaymentUnitOfWork
    {
        private IPaymentRepository? _paymentRepository;
        public IPaymentRepository PaymentRepository => 
            _paymentRepository ??= new PaymentRepository(_dbContext);
        public PaymentUnitOfWork(
            PaymentSvcDbContext dbContext) : base(dbContext)
        {
        }
    }
}