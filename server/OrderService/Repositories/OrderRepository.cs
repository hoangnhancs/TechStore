using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.Repositories;
using OrderService.Data;
using OrderService.Entities;
using OrderService.Repositories.Interface;

namespace OrderService.Repositories
{
    public class OrderRepository : BaseEFRepository<Order, string, OrderSvcDbContext>, IOrderRepository
    {
        public OrderRepository(OrderSvcDbContext context) : base(context)
        {
        }
    }
}