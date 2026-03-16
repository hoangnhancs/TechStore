using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CartService.Data
{
    public class CartSvcDbContext : DbContext
    {
        public CartSvcDbContext(DbContextOptions<CartSvcDbContext> options) : base(options)
        {
        }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
    }
}