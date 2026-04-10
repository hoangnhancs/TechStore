using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReviewService.Data;
using ReviewService.Entities;
using ReviewService.Repositories.Interface;
using Infrastructure.EF.Repositories;

namespace ReviewService.Repositories
{
    public class ReviewRepository : BaseEFRepository<Review, string, ReviewSvcDbContext>, IReviewRepository
    {
        public ReviewRepository(ReviewSvcDbContext dbContext) : base(dbContext)
        {
        }
    }
}