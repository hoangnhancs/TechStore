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
    public class AddressRepository : BaseEFRepository<Address, string, IdentitySvcDbContext>, IAddressRepository
    {
        public AddressRepository(IdentitySvcDbContext dbContext) : base(dbContext)
        {
        }
    }
}