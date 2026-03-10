using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace IdentityService.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IBaseEFRepository<RefreshToken, int>
    {
        Task RevokeAsync(string ipAddress, string? userId = null, string? token = null, string? reason = null);
    }
}