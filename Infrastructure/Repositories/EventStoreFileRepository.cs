using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Exceptions;
using Infrastructure.Model;
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

        public async Task SaveAsync(IEntityId aggregateRootId, int originatingVersion, IReadOnlyCollection<IDomainEvent> events, string aggregateName = "Aggregate Name")
        {
            if (events.Count == 0) return;

            lock (_filepath)
            {
                var existingEvents = LoadAllEvents();

                var aggregateId = aggregateRootId.ToString();

                var currentIndex = existingEvents.LastOrDefault()?.Sequence ?? 0;
                var currentVersion = existingEvents.Where(e => e.AggregateId == aggregateId).OrderBy(e => e.Version).LastOrDefault()?.Version ?? 0;

                if (originatingVersion != currentVersion)
                {
                    throw new InvalidOperationException("Concurrent modification");
                }

                var lines = events.Select(e => 
                {
                    var eventName = _eventRegistry.GetEventName(e);
                    var data = SerializeEvent(e);
                    return $"{++currentIndex};{aggregateId};{aggregateName};{++originatingVersion};{e.CreatedAt.ToString("o")};{Guid.NewGuid()};{eventName};{data}";
                });

                File.AppendAllLines(_filepath, lines);
            }

            await Task.CompletedTask;
        }

        public async Task<IReadOnlyCollection<IDomainEvent>> LoadAsync(IEntityId aggregateRootId)
        {
            var aggregateId = aggregateRootId?.ToString() ?? throw new AggregateRootNotProvidedException("AggregateRootId cannot be null");

            var events = LoadAllEvents().Where(e => e.AggregateId == aggregateId)
                                        .OrderBy(e => e.Version)
                                        .Select(e => DeserializeEvent(e.Name, e.Data))
                                        .ToList()
                                        .AsReadOnly();

            return await Task.FromResult(events);
        }

        public async Task<IReadOnlyCollection<(IDomainEvent Event, int Index)>> GetAllEventsAsync(int startIndex, int max)
        {
            var events = LoadAllEvents().Where(e => e.Sequence >= startIndex)
                                        .Take(max)
                                        .Select(e => (DeserializeEvent(e.Name, e.Data), e.Sequence))
                                        .ToList()
                                        .AsReadOnly();

            return await Task.FromResult(events);
        }

        public async Task<IReadOnlyCollection<(IDomainEvent Event, int Index)>> GetEventsByAggregateNamesAsync(int startIndex, int max, params string[] aggregateNames)
        {
            var events = LoadAllEvents().Where(e => e.Sequence >= startIndex)
                                        .Where(e => aggregateNames.Contains(e.Aggregate))
                                        .Take(max)
                                        .Select(e => (DeserializeEvent(e.Name, e.Data), e.Sequence))
                                        .ToList()
                                        .AsReadOnly();

            return await Task.FromResult(events);
        }

        public async Task<IReadOnlyCollection<(IDomainEvent Event, int Index)>> GetEventsByEventNamesAsync(int startIndex, int max, params string[] eventNames)
        {
            var events = LoadAllEvents().Where(e => e.Sequence >= startIndex)
                                        .Where(e => eventNames.Contains(e.Name))
                                        .Take(max)
                                        .Select(e => (DeserializeEvent(e.Name, e.Data), e.Sequence))
                                        .ToList()
                                        .AsReadOnly();

            return await Task.FromResult(events);
        }

        private IEnumerable<EventStoreDao> LoadAllEvents()
        {
            string[] lines = new string[0];
            lock (_filepath)
            {
                if (File.Exists(_filepath))
                {
                    lines = File.ReadAllLines(_filepath);
                }
            }

            var events = lines
                .Select(l =>
                {
                    var parts = l.Split(';');

                    return new EventStoreDao
                    {
                        Aggregate = parts[2],
                        AggregateId = parts[1],
                        CreatedAt = DateTime.Parse(parts[4]),
                        Data = parts[7],
                        Id = Guid.Parse(parts[5]),
                        Name = parts[6],
                        Sequence = int.Parse(parts[0]),
                        Version = int.Parse(parts[3])
                    };
                });

            return events;
        }

        private string SerializeEvent(IDomainEvent evt)
        { 
            string data = JsonConvert.SerializeObject(evt, Formatting.None, _jsonSerializerSettings);
            return data;
        }

        private IDomainEvent DeserializeEvent(string eventName, string data)
        {
            var eventType = _eventRegistry.GetEventType(eventName);
            var o = JsonConvert.DeserializeObject(data, eventType, _jsonSerializerSettings);
            var evt = o as IDomainEvent;
            return evt;
        }
    }
}
