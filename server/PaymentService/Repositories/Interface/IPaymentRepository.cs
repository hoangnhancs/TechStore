using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace PaymentService.Repositories.Interface
{
    public interface IPaymentRepository : IBaseEFRepository<Payment, string>
    {
        
    }
}