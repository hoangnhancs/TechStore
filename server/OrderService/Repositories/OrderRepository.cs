using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.Repositories;
using OrderService.Data;
using OrderService.Entities;

namespace OrderService.Repositories
{
    public class OrderRepository : BaseEFRepository<Order, string, OrderSvcDbContext>
    {
        public OrderRepository(OrderSvcDbContext context) : base(context)
        {
        }
    }
}