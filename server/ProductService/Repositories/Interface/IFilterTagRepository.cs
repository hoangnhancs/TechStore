using ProductService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace ProductService.Repositories.Interface
{
    public interface IFilterTagRepository : IBaseEFRepository<FilterTag, int>
    {
        Task<List<FilterTag>> GetByCategoryWithValuesAsync(int? categoryId, CancellationToken cancellationToken = default);
    }
}
