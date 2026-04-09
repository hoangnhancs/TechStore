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

        public async Task SetOtherAddressNotDefaultAsync(string userId, CancellationToken cancellationToken)
        {
            var addresses = await GetListAsync(
                predicate: a => a.UserId == userId && a.IsDefault == true,
                cancellationToken: cancellationToken
            );
            foreach (var address in addresses)
            {
                address.IsDefault = false;
            }
        }
    }
}