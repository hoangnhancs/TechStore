using IdentityService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace IdentityService.Repositories.Interfaces
{
    public interface IAddressRepository : IBaseEFRepository<Address, string>
    {
        Task<List<Address>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task SetOtherAddressNotDefaultAsync(string userId, string? currentAddressId, CancellationToken cancellationToken);
    }
}
