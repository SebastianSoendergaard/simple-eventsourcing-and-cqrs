using System.Collections.Generic;
using Core.Person.DomainEvents;
using Tactical.DDD;

namespace Core.Person
{
    public class Person: Tactical.DDD.EventSourcing.AggregateRoot<PersonId>
    {
        public override PersonId Id { get; protected set; }
        public PersonPrivateDataId PersonPrivateDataId { get; private set; }
        public bool IsDeleted { get; private set; }

        public string FirstName { get { return privateData?.FirstName ?? ""; } }
        public string LastName { get { return privateData?.LastName ?? ""; } }
        public Address PersonAddress { get { return privateData?.PersonAddress; } }
        public string DeleteReason { get; set; }

        private PersonPrivateData privateData;

        public Person(IEnumerable<IDomainEvent> events) : base(events)
        {
        }

        private Person(string firstName, string lastName)
        {
            privateData = PersonPrivateData.Create(firstName, lastName);
            PersonPrivateDataId = privateData.Id;
        }

        public static Person CreateNewPerson(string firstName, string lastName)
        {
            var person = new Person(firstName, lastName);
            person.Apply(new PersonCreated(new PersonId().ToString(), person.PersonPrivateDataId.ToString()));
            return person;
        }

        public void ChangePersonAddress(string street,string country, string zipCode, string city)
        {
            privateData.ChangePersonAddress(street, country, zipCode, city);
        }

        public void DeletePerson(string reason)
        {
            Apply(new PersonDeleted(reason));
        }

        public void ApplyPrivateDataEvents(IEnumerable<IDomainEvent> events)
        {
            privateData = new PersonPrivateData(events);
        }

        public PersonPrivateData GetPrivateData()
        {
            return privateData;
        }

        public void On(PersonCreated @event)
        {
            Id = new PersonId(@event.PersonId);
            PersonPrivateDataId = new PersonPrivateDataId(@event.PersonPrivateDataId);
        }

        public void On(PersonDeleted @event)
        {
            DeleteReason = @event.Reason;
            IsDeleted = true;
            privateData = null;
        }
    }
}
