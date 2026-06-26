using IdentityService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace IdentityService.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IBaseEFRepository<RefreshToken, int>
    {
        Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task RevokeAsync(string ipAddress, string? userId = null, string? token = null, string? reason = null);
    }
}
