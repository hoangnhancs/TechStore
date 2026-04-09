using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Core.EF.Domain.Entities
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        string? CreatedBy { get; set; }
        string? UpdatedBy { get; set; }
        bool IsDeleted { get; }
        void MarkAsDeleted();
    }

    public interface IBaseEntity<TId> : IEntity<TId>, IAuditableEntity
    {
    }
}