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
        private IBasketRepository? _basketRepository;
        public IBasketRepository BasketRepository =>
            _basketRepository ??= new BasketRepository(_dbContext);
        public CartUnitOfWork(
            CartSvcDbContext context) : base(context)
        {
        }
    }
}