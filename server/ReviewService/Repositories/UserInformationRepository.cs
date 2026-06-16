using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Infrastructure.EF.Repositories;
using ReviewService.Data;
using ReviewService.Entities;
using ReviewService.Repositories.Interface;
using Shared.Core.EF.Domain.Repositories;

namespace ReviewService.Repositories
{
    public class UserInformationRepository : BaseEFRepository<UserInformation, int, ReviewSvcDbContext>, IUserInformationRepository
    {
        public UserInformationRepository(ReviewSvcDbContext dbContext) : base(dbContext)
        {
        }
    }
}