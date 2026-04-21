using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.UnitOfWork;
using ProductService.Data;
using ProductService.Repositories;
using ProductService.Repositories.Interface;

namespace ProductService.Persistence
{
    /// <summary>
    /// Unit of Work cho Product Service
    /// Kế thừa UnitOfWork<ProductSvcDbContext> để tái sử dụng transaction logic
    /// </summary>
    public class ProductUnitOfWork : UnitOfWork<ProductSvcDbContext>, IProductUnitOfWork
    {
        private IProductRepository? _productRepository;
        private IBannerImageRepository? _bannerImageRepository;
        private IBrandRepository? _brandRepository;
        private ICategoryRepository? _categoryRepository;
        private IFilterTagRepository? _filterTagRepository;
        public IProductRepository ProductRepository => 
            _productRepository ??= new ProductRepository(_dbContext);
        public IBannerImageRepository BannerImageRepository => 
            _bannerImageRepository ??= new BannerImageRepository(_dbContext);
        public IBrandRepository BrandRepository => 
            _brandRepository ??= new BrandRepository(_dbContext);
        public ICategoryRepository CategoryRepository => 
            _categoryRepository ??= new CategoryRepository(_dbContext);
        public IFilterTagRepository FilterTagRepository => 
            _filterTagRepository ??= new FilterTagRepository(_dbContext);

        public ProductUnitOfWork(
            ProductSvcDbContext context) : base(context)
        {
        }
    }
}