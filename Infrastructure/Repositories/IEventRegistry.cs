using System;
using Tactical.DDD;

namespace Infrastructure.Repositories
{
    public interface IEventRegistry
    {
        Type GetEventType(string eventName);

        string GetEventName(IDomainEvent evt);
    }
}
