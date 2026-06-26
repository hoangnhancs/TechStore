using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Entities;
using ProductService.Repositories.Interface;

namespace ProductService.Repositories
{
    public class BrandRepository : BaseEFRepository<Brand, int, ProductSvcDbContext>, IBrandRepository
    {
        public BrandRepository(ProductSvcDbContext context) : base(context)
        {
        }

        public async Task<List<Brand>> GetByCategoryAsync(int? categoryId, CancellationToken cancellationToken = default)
        {
            var query = DbSet.AsQueryable();
            if (categoryId.HasValue)
                query = query.Where(b => b.CategoryId == categoryId.Value);
            return await query.ToListAsync(cancellationToken);
        }
    }
}
