using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.UnitOfWork;
using ProductService.Data;
using ProductService.Repositories.Interface;

namespace ProductService.Persistence
{
    /// <summary>
    /// Unit of Work cho Product Service
    /// Kế thừa UnitOfWork<ProductSvcDbContext> để tái sử dụng transaction logic
    /// </summary>
    public class ProductUnitOfWork : UnitOfWork<ProductSvcDbContext>, IProductUnitOfWork
    {
        public IProductRepository ProductRepository { get; }
        public IBannerImageRepository BannerImageRepository { get; }
        public IBrandRepository BrandRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public IFilterTagRepository FilterTagRepository { get; }

        public ProductUnitOfWork(
            ProductSvcDbContext context,
            IProductRepository productRepository,
            IBannerImageRepository bannerImageRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IFilterTagRepository filterTagRepository
            ) : base(context)  
        {
            ProductRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            BannerImageRepository = bannerImageRepository ?? throw new ArgumentNullException(nameof(bannerImageRepository));    
            BrandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
            CategoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            FilterTagRepository = filterTagRepository ?? throw new ArgumentNullException(nameof(filterTagRepository));
        }
    }
}