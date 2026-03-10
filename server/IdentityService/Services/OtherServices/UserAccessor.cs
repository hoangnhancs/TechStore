using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Services.OtherServices
{
    public class UserAccessor : IUserAccessor
    {
        private readonly IdentitySvcDbContext context;
        private readonly IHttpContextAccessor httpContextAccessor;
        public UserAccessor(IdentitySvcDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
        }
        public async Task<User> GetUserAsync()
        {
            return await context.Users
                .Include(u => u.Image)
                .FirstOrDefaultAsync(u => u.Id == GetUserId()) ?? throw new UnauthorizedAccessException("User not found");
        }

        public string GetUserId()
        {
            return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("No user found");
        }
    }
}