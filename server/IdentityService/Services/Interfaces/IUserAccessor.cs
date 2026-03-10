using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Entities;

namespace IdentityService.Services.Interfaces
{
    public interface IUserAccessor
    {
        string GetUserId();
        Task<User> GetUserAsync();
    }
}