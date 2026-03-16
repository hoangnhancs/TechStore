using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartService.Repositories.Interface;
using Shared.Core.EF.UnitOfWork;

namespace CartService.Persistence
{
    public interface ICartUnitOfWork : IUnitOfWork
    {
        IBasketRepository BasketRepository { get; }
    }
}