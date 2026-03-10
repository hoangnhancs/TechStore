using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductService.Repositories.Interface;
using Shared.Core.EF.UnitOfWork;

namespace ProductService.Persistence
{
    public interface IProductUnitOfWork : IUnitOfWork
    {
        IProductRepository ProductRepository { get; }
        IBannerImageRepository BannerImageRepository { get; }
        IBrandRepository BrandRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IFilterTagRepository FilterTagRepository { get; }
    }
}