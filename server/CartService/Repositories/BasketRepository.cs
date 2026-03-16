using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartService.Data;
using CartService.Entities;
using Infrastructure.EF.Repositories;

namespace CartService.Repositories.Interface
{
    /// <summary>
    /// Repository implementation cho Basket entity
    /// Kế thừa BaseRepository để tái sử dụng CRUD operations
    /// </summary>
    public class BasketRepository : BaseEFRepository<Basket, string, CartSvcDbContext>, IBasketRepository
    {
        public BasketRepository(CartSvcDbContext context) : base(context)
        {
        }       
    }
}