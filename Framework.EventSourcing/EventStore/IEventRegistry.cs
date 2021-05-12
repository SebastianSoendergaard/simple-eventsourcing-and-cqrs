using System;

namespace Framework.DDD.EventStore
{
    public interface IEventRegistry
    {
        Type GetEventType(string eventName);

        string GetEventName(IDomainEvent evt);
    }
}
