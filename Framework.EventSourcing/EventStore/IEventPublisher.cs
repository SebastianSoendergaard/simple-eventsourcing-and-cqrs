using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.DDD.EventStore
{
    public interface IEventPublisher
    {
        // Listen for new events
        event EventHandler<StoredEvent> EventStored;

        // Manually poll for new events, this will ensure at-least-once delivery
        Task<IReadOnlyCollection<StoredEvent>> GetAllEventsAsync(int startIndex, int max);
        Task<IReadOnlyCollection<StoredEvent>> GetEventsByAggregateNamesAsync(int startIndex, int max, params string[] aggregateNames);
        Task<IReadOnlyCollection<StoredEvent>> GetEventsByEventNamesAsync(int startIndex, int max, params string[] eventNames);
    }

    public class StoredEvent
    {
        public StoredEvent(int index, string aggregateName, string eventName, string aggregateRootId, IDomainEvent evt)
        {
            Index = index;
            AggregateName = aggregateName;
            EventName = eventName;
            AggregateRootId = aggregateRootId;
            Event = evt;
        }

        public int Index { get; }
        public string AggregateName { get; }
        public string EventName { get; }
        public string AggregateRootId { get; }
        public IDomainEvent Event { get; }
    }
}
