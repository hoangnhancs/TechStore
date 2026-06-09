using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReviewService.Repositories.Interface;
using Shared.Core.EF.UnitOfWork;

namespace ReviewService.Persistence
{
    public interface IReviewUnitOfWork : IUnitOfWork
    {
        public IReviewRepository ReviewRepository { get; }
        public IUserInformationRepository UserInformationRepository { get; }
    }
}