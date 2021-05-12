// This file was originally taken from https://github.com/aneshas/tactical-ddd

namespace Framework.DDD.EventStore
{
    public interface IAggregateRoot<out TIdentity> : DDD.IAggregateRoot<TIdentity> where TIdentity : IEntityId
    {
        int Version { get; }
    }
}