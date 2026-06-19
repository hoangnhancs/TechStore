using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Entities;
using OrderService.Repositories.Interface;

namespace OrderService.Repositories
{
    public class OrderRepository : BaseEFRepository<Order, string, OrderSvcDbContext>, IOrderRepository
    {
        public OrderRepository(OrderSvcDbContext context) : base(context)
        {
        }

        public async Task<List<Order>> GetListOrdersInDateRangeWithUserInfor(DateTime startDate, DateTime endDate)
        {
            var orders = await _dbContext.Orders.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate).Include(x=>x.User).ToListAsync();
            return orders;
        }
    }
}