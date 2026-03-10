using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Core.EF.Domain.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }

    public abstract record DomainEventBase : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}