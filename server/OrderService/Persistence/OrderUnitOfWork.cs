using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.UnitOfWork;
using OrderService.Data;
using OrderService.Repositories.Interface;

namespace OrderService.Persistence
{
    public class OrderUnitOfWork : UnitOfWork<OrderSvcDbContext>, IOrderUnitOfWork
    {
        public IOrderRepository OrderRepository { get; }
        public OrderUnitOfWork(
            OrderSvcDbContext context, 
            IOrderRepository orderRepository) : base(context)
        {
            OrderRepository = orderRepository;
        }
    }
}