using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Person.DomainEvents;
using Framework.DDD.EventStore;
using Microsoft.Extensions.Logging;

namespace Application.Person.Projections
{
    public class PersonListProjection
    {
        private readonly IEventPublisher eventPublisher;
        private readonly ILogger<PersonListProjection> logger;
        private readonly object syncObj = new object();

        // This should be stored somewhere else
        private bool isInitialized = false;
        private int lastHandledIndex = 0;
        private readonly IDictionary<string, PersonListItem> persons = new Dictionary<string, PersonListItem>();

        public PersonListProjection(IEventPublisher eventPublisher, ILogger<PersonListProjection> logger)
        {
            this.eventPublisher = eventPublisher;
            this.logger = logger;
            this.eventPublisher.EventStored += OnEventStored;

            Init();
        }

        public IReadOnlyList<PersonListItem> GetPersonList()
        {
            lock (syncObj)
            {
                return persons.Values.Select(v => v.Copy()).ToList();
            }
        }

        private void Init()
        {
            _ = Task.Run(async () => 
            {
                int expectedCount = 5;
                int actualCount = expectedCount;
                while (actualCount == expectedCount)
                {
                    var events = await eventPublisher.GetEventsByAggregateNamesAsync(lastHandledIndex + 1, expectedCount, typeof(Core.Person.Person).Name, typeof(Core.Person.PrivateData).Name);
                    foreach (var evt in events)
                    {
                        HandleStoredEvent(evt);
                    }
                    actualCount = events.Count;
                }

                isInitialized = true;
            });
        }

        private void OnEventStored(object sender, StoredEvent storedEvent)
        {
            if (!isInitialized)
            {
                // Event will be handled by the init
                return;
            }

            if (storedEvent.AggregateName != typeof(Core.Person.Person).Name && storedEvent.AggregateName != typeof(Core.Person.PrivateData).Name)
            {
                // Event comes from a irelevant stream
                return;
            }

            HandleStoredEvent(storedEvent);
        }

        private void HandleStoredEvent(StoredEvent storedEvent)
        {
            try
            {
                switch (storedEvent.Event)
                {
                    case PrivateDataCreated evt:
                        HandlePersonPrivateDataCreated(evt);
                        break;
                    case PhoneNumberChanged evt:
                        HandlePhoneNumberChanged(evt);
                        break;
                    case PersonDeleted evt:
                        HandlePersonDeleted(storedEvent, evt);
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

        private void HandlePersonPrivateDataCreated(PrivateDataCreated evt)
        {
            lock (syncObj)
            {
                if (!persons.TryGetValue(evt.PersonId, out var personListItem))
                {
                    personListItem = new PersonListItem { PersonId = evt.PersonId };
                    persons.Add(evt.PersonId, personListItem);
                }

                personListItem.FirstName = evt.FirstName;
                personListItem.LastName = evt.LastName;
            }
        }

        private void HandlePhoneNumberChanged(PhoneNumberChanged evt)
        {
            if (persons.TryGetValue(evt.PersonId, out var personListItem))
            {
                personListItem.PhoneNumber = evt.PhoneNumber;
            }
        }

        private void HandlePersonDeleted(StoredEvent storedEvent, PersonDeleted evt)
        {
            lock (syncObj)
            {
                if (persons.TryGetValue(storedEvent.AggregateRootId, out var personListItem))
                {
                    personListItem.IsDeleted = true;
                    personListItem.DeleteReason = evt.Reason;
                }
            }
        }
    }

    public class PersonListItem
    {
        public string PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public string DeleteReason { get; set; }

        public PersonListItem Copy()
        {
            return this.MemberwiseClone() as PersonListItem;
        }
    }
}
