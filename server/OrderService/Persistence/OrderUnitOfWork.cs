using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.UnitOfWork;
using OrderService.Data;

namespace OrderService.Persistence
{
    public class OrderUnitOfWork : UnitOfWork<OrderSvcDbContext>, IOrderUnitOfWork
    {
        public OrderUnitOfWork(OrderSvcDbContext context) : base(context)
        {
        }
    }
}