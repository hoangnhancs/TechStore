using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PaymentService.Entities;
using PaymentService.Repositories.Interface;
using Infrastructure.EF.Repositories;
using PaymentService.Data;

namespace PaymentService.Repositories
{
    public class PaymentRepository : BaseEFRepository<Payment, string, PaymentSvcDbContext>, IPaymentRepository
    {
        public PaymentRepository(PaymentSvcDbContext dbContext) : base(dbContext)
        {
        }
    }
}