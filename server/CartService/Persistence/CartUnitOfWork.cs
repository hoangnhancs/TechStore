using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartService.Data;
using CartService.Repositories.Interface;
using Infrastructure.EF.UnitOfWork;

namespace CartService.Persistence
{
    public class CartUnitOfWork : UnitOfWork<CartSvcDbContext>, ICartUnitOfWork
    {
        public IBasketRepository BasketRepository { get; }
        public CartUnitOfWork(
            CartSvcDbContext context, 
            IBasketRepository basketRepository) : base(context)
        {
            BasketRepository = basketRepository;
        }
    }
}