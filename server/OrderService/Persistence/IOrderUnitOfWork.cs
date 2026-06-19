using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderService.Repositories.Interface;
using Shared.Core.EF.UnitOfWork;

namespace OrderService.Persistence
{
    public interface IOrderUnitOfWork : IUnitOfWork
    {
        IOrderRepository OrderRepository { get; }
        IUserInformationRepository UserInformationRepository { get; }

    }
}