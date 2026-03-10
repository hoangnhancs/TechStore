using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.DTOs;
using IdentityService.Entities;

namespace IdentityService.Services.Interfaces
{
    public interface ITokenServices
    {
        Task<AccessTokenResult> CreateAccessTokenAsync(User user);
        RefreshToken CreateRefreshToken(User user, string ipAddress);
    }
}