using ProductService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace ProductService.Repositories.Interface
{
    public interface IBrandRepository : IBaseEFRepository<Brand, int>
    {
        Task<List<Brand>> GetByCategoryAsync(int? categoryId, CancellationToken cancellationToken = default);
    }
}
