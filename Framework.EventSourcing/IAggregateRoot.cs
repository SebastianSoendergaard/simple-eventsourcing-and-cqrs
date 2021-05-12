// This file was originally taken from https://github.com/aneshas/tactical-ddd

using System.Collections.Generic;

namespace Framework.DDD
{
    public interface IAggregateRoot<out TIdentity> : IEntity<TIdentity> where TIdentity : IEntityId 
    {
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    }
}