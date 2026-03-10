using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Events;

namespace Shared.Core.EF.Domain.Entities
{
    public abstract class AggregateRoot<TId> : BaseEntity<TId>
    {
         private readonly List<IDomainEvent> _domainEvents = new();

        /// <summary>
        /// Gets the domain events that occurred during the lifetime of this aggregate
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected AggregateRoot() : base() { }

        protected AggregateRoot(TId id) : base(id) { }

        /// <summary>
        /// Add a domain event to be dispatched when the aggregate is persisted
        /// </summary>
        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Clear all domain events (called after dispatching)
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}