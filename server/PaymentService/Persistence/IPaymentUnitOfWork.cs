using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.Repositories.Interface;
using Shared.Core.EF.UnitOfWork;

namespace PaymentService.Persistence
{
    public interface IPaymentUnitOfWork : IUnitOfWork
    {
        public IPaymentRepository PaymentRepository { get; }
    }
}