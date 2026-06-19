using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.UnitOfWork;
using OrderService.Data;
using OrderService.Repositories;
using OrderService.Repositories.Interface;

namespace OrderService.Persistence
{
    public class OrderUnitOfWork : UnitOfWork<OrderSvcDbContext>, IOrderUnitOfWork
    {
        private IOrderRepository? _orderRepository;
        public IOrderRepository OrderRepository => 
            _orderRepository ??= new OrderRepository(_dbContext);
        private IUserInformationRepository? _userInformationRepository;
        public IUserInformationRepository UserInformationRepository => 
            _userInformationRepository ??= new UserInformationRepository(_dbContext);
        public OrderUnitOfWork(
            OrderSvcDbContext context) : base(context)
        {
        }
    }
}