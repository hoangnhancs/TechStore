using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Core.EF.Domain.Entities;

namespace Infrastructure.EF.Data
{
    public abstract class BaseDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        protected BaseDbContext(DbContextOptions options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            AuditHelper.ApplyAudit(ChangeTracker, userId);
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}