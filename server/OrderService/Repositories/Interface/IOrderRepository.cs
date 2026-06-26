using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace OrderService.Repositories.Interface
{
    public interface IOrderRepository : IBaseEFRepository<Order, string>
    {
        Task<List<Order>> GetListOrdersInDateRangeWithUserInfor(DateTime startDate, DateTime endDate, Order.OrderStatus? status = null);
        Task<List<Order>> GetListOrdersWaitingForConfirmation(DateTime? startDate = null, DateTime? endDate = null);
    }
}