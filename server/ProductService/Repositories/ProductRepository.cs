using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Entities;
using ProductService.Repositories.Interface;

namespace ProductService.Repositories
{
    /// <summary>
    /// Repository implementation cho Product entity
    /// Kế thừa BaseRepository để tái sử dụng CRUD operations
    /// </summary>
    public class ProductRepository : BaseEFRepository<Product, string, ProductSvcDbContext>, IProductRepository
    {
        public ProductRepository(ProductSvcDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy top 10 sản phẩm theo từng category (sắp xếp theo sản phẩm mới nhất)
        /// </summary>
        public async Task<List<Product>> GetTop10ProductPerCategory(CancellationToken cancellationToken)
        {
            // var result = new List<Product>();

            // // Lấy danh sách categories
            // var categoryIds = await Context.Categories
            //     .Select(c => c.Id)
            //     .ToListAsync(cancellationToken);

            // // Với mỗi category, lấy top 10 products (sắp xếp theo ngày tạo mới nhất)
            // foreach (var categoryId in categoryIds)
            // {
            //     var topProducts = await DbSet
            //         .Where(p => p.CategoryId == categoryId && p.IsActive)
            //         .OrderByDescending(p => p.UnitSold)
            //         .Take(10)
            //         .ToListAsync(cancellationToken);

            //     result.AddRange(topProducts);
            // }

            var sqlQuery = @"
                SELECT *
                FROM (
                    SELECT *, ROW_NUMBER() OVER (PARTITION BY category_id ORDER BY created_at DESC) as rn
                    FROM products
                    WHERE is_active = true
                ) as topProducts
                WHERE rn <= 10
            ";

            var result = await DbSet.FromSqlRaw(sqlQuery).Include(p => p.Category).ToListAsync(cancellationToken);

            return result;
        }

        public async Task<Product?> GetProductWithDetailsAsync(string productId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(p => p.DetailImages)
                .Include(p => p.Attributes)
                .Include(p => p.ProductFilterTagValues)
                    .ThenInclude(pftv => pftv.FilterTagValue)
                        .ThenInclude(ftv => ftv!.FilterTag)
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        }

        public async Task<Product?> GetProductForDisplayAsync(string productId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.DisplayTags)
                .Include(p => p.DetailImages)
                .Include(p => p.Attributes)
                .Include(p => p.ProductFilterTagValues)
                    .ThenInclude(pftv => pftv.FilterTagValue)
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        }

        public async Task<List<Product>> GetActiveProductsAsync(DateTime? updatedAfter, CancellationToken cancellationToken = default)
        {
            var query = DbSet
                .Where(p => p.IsActive && !p.IsDeleted);

            if (updatedAfter.HasValue)
                query = query.Where(p => p.UpdatedAt > updatedAfter.Value);

            return await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Attributes)
                .Include(p => p.ProductFilterTagValues)
                    .ThenInclude(pftv => pftv.FilterTagValue)
                .Include(p => p.DisplayTags)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Product>> GetActiveByCategoryOrBrandAsync(int? categoryId, int? brandId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(p => p.IsActive
                    && (categoryId == null || p.CategoryId == categoryId)
                    && (brandId == null || p.BrandId == brandId))
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.DisplayTags)
                .Include(p => p.ProductFilterTagValues)
                .ToListAsync(cancellationToken);
        }
    }
}
