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

        public async Task<List<Order>> GetListOrdersInDateRangeWithUserInfor(DateTime startDate, DateTime endDate, Order.OrderStatus? status = null)
        {
            var query = _dbContext.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            return await query
                .Include(x => x.User)
                .Include(x => x.Items)
                .ToListAsync();
        }

        public async Task<List<Order>> GetListOrdersWaitingForConfirmation(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.Orders
                .Where(o => o.Status == Order.OrderStatus.WaitingForConfirmation);

            if (startDate.HasValue)
                query = query.Where(o => o.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.CreatedAt <= endDate.Value);

            return await query
                .Include(x => x.User)
                .Include(x => x.Items)
                .ToListAsync();
        }
    }
}