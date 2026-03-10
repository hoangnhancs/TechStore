using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Repositories.Interfaces;
using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories
{
    public class RefreshTokenRepository : BaseEFRepository<RefreshToken, int, IdentitySvcDbContext>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(IdentitySvcDbContext context) : base(context)
        {
        }
        public async Task RevokeAsync(string ipAddress, string? userId = null, string? token = null, string? reason = null)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(token))
                throw new ArgumentException("UserId or token must be provided.");

            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentException("IP address must be provided.");

            if (!string.IsNullOrEmpty(userId))
            {
                var userTokens = await DbSet
                    .Where(rt => rt.UserId == userId && !rt.Revoked.HasValue && rt.Expires > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var userToken in userTokens)
                {
                    userToken.Revoked = DateTime.UtcNow;
                    userToken.RevokedByIp = ipAddress;
                    userToken.ReasonRevoked = reason;
                }

                return;
            }
            else if (!string.IsNullOrEmpty(token))
            {
                var refreshToken = await DbSet
                    .FirstOrDefaultAsync(rt => rt.Token == token && !rt.Revoked.HasValue && rt.Expires > DateTime.UtcNow);

                if (refreshToken != null)
                {
                    refreshToken.Revoked = DateTime.UtcNow;
                    refreshToken.RevokedByIp = ipAddress;
                    refreshToken.ReasonRevoked = reason;
                }

                return;
            }
        }
    }
}