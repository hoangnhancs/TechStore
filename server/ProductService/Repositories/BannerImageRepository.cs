using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.Repositories;
using ProductService.Data;
using ProductService.Entities;
using ProductService.Repositories.Interface;

namespace ProductService.Repositories
{
    /// <summary>
    /// Repository implementation cho BannerImage entity
    /// Chỉ sử dụng base CRUD operations từ BaseRepository
    /// </summary>
    public class BannerImageRepository : BaseEFRepository<BannerImage, int, ProductSvcDbContext>, IBannerImageRepository
    {
        public BannerImageRepository(ProductSvcDbContext context) : base(context)
        {
        }

        // Không có custom methods - sử dụng methods từ BaseRepository
    }
}
