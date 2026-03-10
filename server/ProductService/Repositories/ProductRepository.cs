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

        // Các methods khác trong IProductRepository (đang comment) có thể implement ở đây
        // Ví dụ:
        // public async Task<Product?> GetProductByIdWithDetailFilterTagsAsync(string productId, CancellationToken ct)
        // {
        //     return await DbSet
        //         .Include(p => p.FilterTags)
        //         .FirstOrDefaultAsync(p => p.Id == productId, ct);
        // }
    }
}
