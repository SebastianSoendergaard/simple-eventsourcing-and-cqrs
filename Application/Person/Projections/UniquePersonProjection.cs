using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Person.DomainEvents;
using Framework.DDD.EventStore;
using Microsoft.Extensions.Logging;

namespace Application.Person.Projections
{
    public class UniquePersonProjection
    {
        private readonly IEventPublisher eventPublisher;
        private readonly ILogger<UniquePersonProjection> logger;
        private readonly object syncObj = new object();
        private Task eventFetcher;

        // TODO: This should be stored someware to avoid need for reading all events from start on init
        private int lastHandledIndex = 0;
        private readonly Dictionary<string, string> personNames = new Dictionary<string, string>();

        public UniquePersonProjection(IEventPublisher eventPublisher, ILogger<UniquePersonProjection> logger)
        {
            this.eventPublisher = eventPublisher;
            this.logger = logger;
            this.eventPublisher.EventStored += (s, e) => OnStoredEvent(e);

            StartEventFetching();
        }

        public bool IsPersonUnique(string firstname, string lastname, string personId = null)
        {
            lock (syncObj)
            {
                if (personNames.TryGetValue(CreateName(firstname, lastname), out var existingPersonId))
                {
                    return personId != null && personId == existingPersonId;
                }
                return true;
            }
        }

        public event EventHandler<string> UniquePersonConstraintBroken;

        private void StartEventFetching()
        {
            if (eventFetcher != null && !eventFetcher.IsCompleted)
            {
                return;
            }

            eventFetcher = Task.Run(async () =>
            {
                int expectedCount = 5; // This number should be much larger, but use small number here to make it possible to follow the mechanisms of the fetching
                var events = await eventPublisher.GetEventsByEventNamesAsync(lastHandledIndex + 1, expectedCount, typeof(PersonPrivateDataCreated).Name, typeof(PersonDeleted).Name);
                foreach (var evt in events)
                {
                    HandleStoredEvent(evt);
                }

                return events.Count == expectedCount;
            })
            .ContinueWith(async hasMore => 
            {
                if (await hasMore)
                {
                    await Task.Delay(1000); 
                    StartEventFetching();
                }
            });
        }

        private void OnStoredEvent(StoredEvent storedEvent)
        {
            // In this case we do not handle the event directly
            // we just use it as a trigger for fetching the newest events 
            StartEventFetching();
        }

        private void HandleStoredEvent(StoredEvent storedEvent)
        {
            try
            {
                switch (storedEvent.Event)
                {
                    case PersonPrivateDataCreated evt:
                        HandlePersonPrivateDataCreated(evt);
                        break;
                    case PersonDeleted evt:
                        HandlePersonDeleted(storedEvent);
                        break;
                }
            }
            catch (Exception ex)
            {
                // TODO: We need to handle this in some way. 
                // It will probably not help to try again and we can not block the entire stream! So lets ignore for now, but maybe we should raise a flag for manual solving.
                logger.LogCritical(ex, "Exception while applying event");
            }

            lastHandledIndex = storedEvent.Index;
        }

        private void HandlePersonPrivateDataCreated(PersonPrivateDataCreated evt)
        {
            bool isUnique = false;

            lock (syncObj)
            {
                isUnique = IsPersonUnique(evt.FirstName, evt.LastName);
                if (isUnique)
                {
                    personNames.Add(CreateName(evt.FirstName, evt.LastName), evt.PersonId);
                }
            }

            if (!isUnique)
            {
                UniquePersonConstraintBroken?.Invoke(this, evt.PersonId);
            }
        }

        private void HandlePersonDeleted(StoredEvent storedEvent)
        {
            lock (syncObj)
            {
                var name = personNames.Values.FirstOrDefault(v => v == storedEvent.AggregateRootId);
                if (name != null)
                {
                    personNames.Remove(name);
                }
            }
        }

        private string CreateName(string firstname, string lastname)
        { 
            return firstname + lastname;
        }
    }
}
