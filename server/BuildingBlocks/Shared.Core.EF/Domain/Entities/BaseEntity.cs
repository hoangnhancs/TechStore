using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Core.EF.Domain.Entities
{
    public class BaseEntity<TId> : IBaseEntity<TId>
    {
        public TId Id { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; protected set; } = false;

        protected BaseEntity() { }

        protected BaseEntity(TId id)
        {
            Id = id;
        }

        public void SetUpdatedAt(DateTime? updatedAt = null, string? updatedBy = null)
        {
            UpdatedAt = updatedAt ?? DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
            SetUpdatedAt();
        }
    }
}