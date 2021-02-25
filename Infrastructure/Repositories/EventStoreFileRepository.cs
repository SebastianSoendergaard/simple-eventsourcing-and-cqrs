using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Exceptions;
using Newtonsoft.Json;
using Tactical.DDD;

namespace Infrastructure.Repositories
{
    public class EventStoreFileRepository : IEventStore
    {
        private readonly string _filepath;
        private readonly IEventRegistry _eventRegistry;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore
        };

        public EventStoreFileRepository(string filepath, IEventRegistry eventRegistry)
        {
            _filepath = filepath;
            _eventRegistry = eventRegistry;
        }

        public Task<IReadOnlyCollection<IDomainEvent>> LoadAsync(IEntityId aggregateRootId)
        {
            if (aggregateRootId == null) throw new AggregateRootNotProvidedException("AggregateRootId cannot be null");

            var lines = File.ReadAllLines(_filepath);

            var id = aggregateRootId.ToString();

            var aggregateLines = lines.Where(l => l.StartsWith(id));

            var events = aggregateLines.Select(l => EventFromString(l)).OrderBy(e => e.Version);

            IReadOnlyCollection<IDomainEvent> readOnly = events.Select(e => e.DomainEvent).ToList().AsReadOnly();

            return Task.FromResult(readOnly);
        }

        public async Task SaveAsync(IEntityId aggregateId, int originatingVersion, IReadOnlyCollection<IDomainEvent> events, string aggregateName = "Aggregate Name")
        {
            if (events.Count == 0) return;

            // TODO: make concurrency check: aggregateId + version must be unique 

            File.AppendAllLines(_filepath, events.Select(e => EventToString(aggregateId, aggregateName, ++originatingVersion, e)));

            await Task.CompletedTask;
        }

        private string EventToString(IEntityId aggregateId, string aggregateName, int version, IDomainEvent evt)
        {
            string eventName = _eventRegistry.GetEventName(evt);
            string data = JsonConvert.SerializeObject(evt, Formatting.None, _jsonSerializerSettings);

            return $"{aggregateId};{aggregateName};{version};{evt.CreatedAt.ToString("o")};{Guid.NewGuid()};{eventName};{data}";
        }

        private (int Version, IDomainEvent DomainEvent) EventFromString(string str)
        {
            var parts = str.Split(';');

            var eventType = _eventRegistry.GetEventType(parts[5]);

            var o = JsonConvert.DeserializeObject(parts[6], eventType, _jsonSerializerSettings);
            var evt = o as IDomainEvent;

            return (int.Parse(parts[2]), evt);
        }
    }
}
