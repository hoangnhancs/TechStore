using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace CartService.Repositories.Interface
{
    public interface IBasketRepository : IBaseEFRepository<Basket, string>
    {
        
    }
}