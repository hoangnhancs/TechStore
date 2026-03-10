using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace IdentityService.Repositories.Interfaces
{
    public interface IAddressRepository : IBaseEFRepository<Address, string>
    {
        
    }
}