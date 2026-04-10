using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReviewService.Data;
using ReviewService.Repositories.Interface;
using Infrastructure.EF.UnitOfWork;

namespace ReviewService.Persistence
{
    public class ReviewUnitOfWork : UnitOfWork<ReviewSvcDbContext>, IReviewUnitOfWork
    {
        public IReviewRepository ReviewRepository { get; }
        public ReviewUnitOfWork(ReviewSvcDbContext context,
            IReviewRepository reviewRepository) : base(context)
        {
            ReviewRepository = reviewRepository;
        }
    }
}