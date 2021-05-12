using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Exceptions;
using Framework.DDD;
using Framework.DDD.EventStore;
using Infrastructure.Model;
using Newtonsoft.Json;

namespace Infrastructure.EventStore
{
    public class EventStoreFileRepository : IEventStore, IEventPublisher
    {
        private readonly string _filepath;
        private readonly IEventRegistry _eventRegistry;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore
        };

        public event EventHandler<StoredEvent> EventStored;

        public EventStoreFileRepository(string filepath, IEventRegistry eventRegistry)
        {
            _filepath = filepath;
            _eventRegistry = eventRegistry;
        }

        public async Task SaveAsync(IEntityId aggregateRootId, int originatingVersion, IReadOnlyCollection<IDomainEvent> events, string aggregateName = "Aggregate Name")
        {
            if (events.Count == 0) return;

            var eventsToPublish = new List<StoredEvent>();

            lock (_filepath)
            {
                var existingEvents = LoadAllEvents();

                var aggregateId = aggregateRootId.ToString();

                var currentIndex = existingEvents.LastOrDefault()?.Sequence ?? 0;
                var currentVersion = existingEvents.Where(e => e.AggregateId == aggregateId).OrderBy(e => e.Version).LastOrDefault()?.Version ?? 0;

                if (originatingVersion != currentVersion)
                {
                    throw new ValidationException("Concurrent modification");
                }

                var lines = new List<string>();

                foreach (var evt in events)
                {
                    var eventName = _eventRegistry.GetEventName(evt);
                    var data = SerializeEvent(evt);
                    lines.Add($"{++currentIndex};{aggregateId};{aggregateName};{++originatingVersion};{evt.CreatedAt.ToString("o")};{Guid.NewGuid()};{eventName};{data}");
                    eventsToPublish.Add(new StoredEvent(currentIndex, aggregateName, eventName, aggregateId, evt));
                }

                File.AppendAllLines(_filepath, lines);
            }

            // Publish events as fire and forget
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000); // Add small delay to provoke eventually consistency 
                eventsToPublish.ForEach(e => EventStored?.Invoke(this, e));
            });

            await Task.CompletedTask;
        }

        public async Task DeleteAsync(IEntityId aggregateRootId)
        {
            var aggregateId = $";{aggregateRootId?.ToString()};";

            lock (_filepath)
            {
                var lines = File.ReadAllLines(_filepath);
                var filteredLines = lines.Where(l => !l.Contains(aggregateId));
                File.WriteAllLines(_filepath, filteredLines);
            }

            await Task.CompletedTask;
        }

        public async Task<IReadOnlyCollection<IDomainEvent>> LoadAsync(IEntityId aggregateRootId)
        {
            var aggregateId = aggregateRootId?.ToString() ?? throw new ValidationException("AggregateRootId cannot be null");

            var events = LoadAllEvents().Where(e => e.AggregateId == aggregateId)
                                        .OrderBy(e => e.Version)
                                        .Select(e => DeserializeEvent(e.Name, e.Data))
                                        .ToList()
                                        .AsReadOnly();

            return await Task.FromResult(events);
        }

        public async Task<IReadOnlyCollection<StoredEvent>> GetAllEventsAsync(int startIndex, int max)
        {
            var events = LoadAllEvents().Where(e => e.Sequence >= startIndex)
                                        .Take(max)
                                        .Select(e => new StoredEvent(e.Sequence, e.Aggregate, e.Name, e.AggregateId, DeserializeEvent(e.Name, e.Data)))
                                        .ToList()
                                        .AsReadOnly();

            return await Task.FromResult(events);
        }

        public async Task<IReadOnlyCollection<StoredEvent>> GetEventsByAggregateNamesAsync(int startIndex, int max, params string[] aggregateNames)
        {
            var events = LoadAllEvents().Where(e => e.Sequence >= startIndex)
                                        .Where(e => aggregateNames.Contains(e.Aggregate))
                                        .Take(max)
                                        .Select(e => new StoredEvent(e.Sequence, e.Aggregate, e.Name, e.AggregateId, DeserializeEvent(e.Name, e.Data)))
                                        .ToList()
                                        .AsReadOnly();

            return await Task.FromResult(events);
        }

        public async Task<IReadOnlyCollection<StoredEvent>> GetEventsByEventNamesAsync(int startIndex, int max, params string[] eventNames)
        {
            var events = LoadAllEvents().Where(e => e.Sequence >= startIndex)
                                        .Where(e => eventNames.Contains(e.Name))
                                        .Take(max)
                                        .Select(e => new StoredEvent(e.Sequence, e.Aggregate, e.Name, e.AggregateId, DeserializeEvent(e.Name, e.Data)))
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
                .Where(l => !string.IsNullOrWhiteSpace(l))
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
