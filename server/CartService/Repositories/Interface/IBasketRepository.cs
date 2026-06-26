using CartService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace CartService.Repositories.Interface
{
    public interface IBasketRepository : IBaseEFRepository<Basket, string>
    {
        Task<Basket?> GetByUserIdWithItemsAsync(string userId, CancellationToken cancellationToken = default);
    }
}
