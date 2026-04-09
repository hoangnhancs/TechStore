using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Core.EF.Domain.Entities;

namespace Infrastructure.EF.Data
{
    public static class AuditHelper
    {
        public static void ApplyAudit(ChangeTracker changeTracker, string? userId)
        {
            foreach (var entry in changeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedBy = userId;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
