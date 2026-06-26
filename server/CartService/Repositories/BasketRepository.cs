using CartService.Data;
using CartService.Entities;
using CartService.Repositories.Interface;
using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CartService.Repositories.Interface
{
    public class BasketRepository : BaseEFRepository<Basket, string, CartSvcDbContext>, IBasketRepository
    {
        public BasketRepository(CartSvcDbContext context) : base(context)
        {
        }

        public async Task<Basket?> GetByUserIdWithItemsAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(x => x.IsDeleted == false)
                .Include(b => b.Items)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);
        }
    }
}
