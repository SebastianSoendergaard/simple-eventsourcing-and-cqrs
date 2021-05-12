using System.Collections.Generic;
using Core.Exceptions;
using Core.Person.DomainEvents;
using Framework.DDD;

namespace Core.Person
{
    public class Person: Framework.DDD.EventStore.AggregateRoot<PersonId>
    {
        public override PersonId Id { get; protected set; }
        public PersonPrivateDataId PersonPrivateDataId { get; private set; }
        public bool IsDeleted { get { return privateData == null; } }
        public string DeleteReason { get; private set; }

        // Private data
        public string FirstName { get { return privateData?.FirstName ?? ""; } }
        public string LastName { get { return privateData?.LastName ?? ""; } }
        public Address PersonAddress { get { return privateData?.PersonAddress; } }
        public string PhoneNumber { get { return privateData?.PhoneNumber ?? ""; } }

        private PersonPrivateData privateData;

        public Person(IEnumerable<IDomainEvent> events) : base(events)
        {
        }

        private Person(PersonId personId, string firstName, string lastName)
        {
            privateData = PersonPrivateData.Create(personId, firstName, lastName);
            PersonPrivateDataId = privateData.Id;
        }

        public static Person CreateNewPerson(string firstName, string lastName)
        {
            var personId = new PersonId();
            var person = new Person(personId, firstName, lastName);
            person.Apply(new PersonCreated(personId.ToString(), person.PersonPrivateDataId.ToString()));
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
            privateData?.PrepareDelete();
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

        public void On(PersonCreated evt)
        {
            Id = new PersonId(evt.PersonId);
            PersonPrivateDataId = new PersonPrivateDataId(evt.PersonPrivateDataId);
        }

        public void On(PersonDeleted evt)
        {
            DeleteReason = evt.Reason;
            privateData = null;
        }
    }
}
