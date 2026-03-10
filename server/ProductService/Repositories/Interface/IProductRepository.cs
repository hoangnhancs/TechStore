using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace ProductService.Repositories.Interface
{
    public interface IProductRepository : IBaseEFRepository<Product, string>
    {
        // Task<Product?> GetProductByIdAsync(string productId, CancellationToken cancellationToken);
        // Task<List<Product>> GetAllProducts(CancellationToken cancellationToken);
        // Task<Product?> GetProductByIdWithDetailFilterTagsAsync(string productId, CancellationToken cancellationToken);
        Task<List<Product>> GetTop10ProductPerCategory(CancellationToken cancellationToken);
        // Task<List<Product>> GetProductsByCategory(int categoryId, CancellationToken cancellationToken);
        // Task<Product> AddNewProduct(Product product, CancellationToken cancellationToken);
        // Task UpdateProductAsync(Product product, DateTime? UpdatedAt, CancellationToken cancellationToken);
        // Task UpdateProductQuantityAsync(string productId, int quantity, string mode, CancellationToken cancellationToken); // mode is add or subtract
        // Task<List<Product>> GetTop10SoldProducts(CancellationToken cancellationToken);
    }
}