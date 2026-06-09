using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReviewService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace ReviewService.Repositories.Interface
{
    public interface IUserInformationRepository : IBaseEFRepository<UserInformation, int>
    {
        
    }
}