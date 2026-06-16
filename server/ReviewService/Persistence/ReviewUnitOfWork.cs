using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReviewService.Data;
using ReviewService.Repositories.Interface;
using Infrastructure.EF.UnitOfWork;
using ReviewService.Repositories;

namespace ReviewService.Persistence
{
    public class ReviewUnitOfWork : UnitOfWork<ReviewSvcDbContext>, IReviewUnitOfWork
    {
        private IReviewRepository? _reviewRepository;
        private IUserInformationRepository? _userInformationRepository;
        public IReviewRepository ReviewRepository => 
            _reviewRepository ??= new ReviewRepository(_dbContext);
        private IUserInformationRepository? _userInformationRepository;
        public IUserInformationRepository UserInformationRepository =>
            _userInformationRepository ??= new UserInformationRepository(_dbContext);
        public ReviewUnitOfWork(ReviewSvcDbContext context) : base(context)
        {
        }
    }
}