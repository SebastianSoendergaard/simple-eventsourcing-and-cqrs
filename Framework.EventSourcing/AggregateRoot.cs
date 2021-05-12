﻿// This file was originally taken from https://github.com/aneshas/tactical-ddd

using System.Collections.Generic;

namespace Framework.DDD
{
    public abstract class AggregateRoot<TIdentity> : Entity<TIdentity>, IAggregateRoot<TIdentity> where TIdentity : IEntityId
    {
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

        protected AggregateRoot() { }

        protected void AddDomainEvent(IDomainEvent @event) =>
            _domainEvents.Add(@event);

        protected void RemoveDomainEvent(IDomainEvent @event) =>
            _domainEvents.Remove(@event);

        protected void ClearDomainEvents() =>
            _domainEvents.Clear();

        public IReadOnlyCollection<IDomainEvent> DomainEvents =>
            _domainEvents.AsReadOnly();
    }
}
