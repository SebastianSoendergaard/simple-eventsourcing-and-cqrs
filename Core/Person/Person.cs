using System.Collections.Generic;
using Core.Exceptions;
using Core.Person.DomainEvents;
using Framework.DDD;

namespace Core.Person
{
    public class Person: Framework.DDD.EventStore.AggregateRoot<PersonId>
    {
        public override PersonId Id { get; protected set; }
        public PrivateDataId PrivateDataId { get; private set; }
        public bool IsDeleted { get { return privateData == null; } }
        public string DeleteReason { get; private set; }

        // Private data
        public string FirstName { get { return privateData?.FirstName; } }
        public string LastName { get { return privateData?.LastName; } }
        public Address PersonAddress { get { return privateData?.Address; } }
        public string PhoneNumber { get { return privateData?.PhoneNumber; } }

        private PrivateData privateData;

        public Person(IEnumerable<IDomainEvent> events) : base(events)
        {
        }

        private Person(PersonId personId, string firstName, string lastName)
        {
            privateData = PrivateData.Create(personId, firstName, lastName);
            PrivateDataId = privateData.Id;
        }

        public static Person CreateNewPerson(string firstName, string lastName)
        {
            var personId = new PersonId();
            var person = new Person(personId, firstName, lastName);
            person.Apply(new PersonCreated(personId.ToString()));
            person.Apply(new PrivateDataAdded(person.PrivateDataId.ToString()));
            return person;
        }

        public void ChangePersonAddress(string street,string country, string zipCode, string city)
        {
            if (privateData == null) throw new NotFoundException("Person can not be changed as it has been deleted");
            privateData.ChangePersonAddress(street, country, zipCode, city);
        }

        public void ChangePhoneNumber(string phoneNumber)
        {
            if (privateData == null) throw new NotFoundException("Person can not be changed as it has been deleted");
            privateData.ChangePhoneNumber(phoneNumber);
        }

        public void DeletePerson(string reason)
        {
            Apply(new PrivateDataRemoved());
            Apply(new PersonDeleted(reason));
        }

        public void ApplyPrivateDataEvents(IEnumerable<IDomainEvent> events)
        {
            privateData = new PrivateData(events);
        }

        public PrivateData GetPrivateData()
        {
            return privateData;
        }

        public void On(PersonCreated evt)
        {
            Id = new PersonId(evt.PersonId);
        }

        public void On(PrivateDataAdded evt)
        {
            PrivateDataId = new PrivateDataId(evt.PrivateDataId);
        }

        public void On(PrivateDataRemoved evt)
        {
            PrivateDataId = null;
            privateData = null;
        }

        public void On(PersonDeleted evt)
        {
            DeleteReason = evt.Reason;
        }
    }
}
