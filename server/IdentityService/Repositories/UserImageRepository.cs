using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Repositories.Interfaces;
using Infrastructure.EF.Repositories;

namespace IdentityService.Repositories
{
    public class UserImageRepository : BaseEFRepository<UserImage, string, IdentitySvcDbContext>, IUserImageRepository
    {
        public UserImageRepository(IdentitySvcDbContext dbContext) : base(dbContext)
        {
        }
    }
}