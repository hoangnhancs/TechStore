using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace OrderService.Repositories.Interface
{
    public interface IOrderHistoryRepository : IBaseEFRepository<OrderStatusHistory, int>
    {
        
    }
}