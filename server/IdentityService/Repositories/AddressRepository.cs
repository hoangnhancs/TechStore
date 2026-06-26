using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Repositories.Interfaces;
using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories
{
    public class AddressRepository : BaseEFRepository<Address, string, IdentitySvcDbContext>, IAddressRepository
    {
        public AddressRepository(IdentitySvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Address>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(a => a.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task SetOtherAddressNotDefaultAsync(string userId, string? currentAddressId, CancellationToken cancellationToken)
        {
            var addresses = await DbSet
                .Where(a => a.UserId == userId
                    && a.IsDefault == true
                    && (string.IsNullOrEmpty(currentAddressId) || a.Id != currentAddressId))
                .ToListAsync(cancellationToken);

            foreach (var address in addresses)
                address.IsDefault = false;
        }
    }
}
