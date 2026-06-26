using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Entities;
using ProductService.Repositories.Interface;

namespace ProductService.Repositories
{
    public class FilterTagRepository : BaseEFRepository<FilterTag, int, ProductSvcDbContext>, IFilterTagRepository
    {
        public FilterTagRepository(ProductSvcDbContext context) : base(context)
        {
        }

        public async Task<List<FilterTag>> GetByCategoryWithValuesAsync(int? categoryId, CancellationToken cancellationToken = default)
        {
            var query = DbSet.Include(ft => ft.Values).AsQueryable();
            if (categoryId.HasValue)
                query = query.Where(ft => ft.CategoryId == categoryId.Value);
            return await query.ToListAsync(cancellationToken);
        }
    }
}
