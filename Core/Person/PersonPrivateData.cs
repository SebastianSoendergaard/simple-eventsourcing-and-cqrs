using System.Collections.Generic;
using Core.Person.DomainEvents;
using Tactical.DDD;

namespace Core.Person
{
    public class PersonPrivateData : Tactical.DDD.EventSourcing.AggregateRoot<PersonPrivateDataId>
    {
        public override PersonPrivateDataId Id { get; protected set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Address PersonAddress { get; private set; }

        public PersonPrivateData(IEnumerable<IDomainEvent> events) : base(events)
        {
        }

        private PersonPrivateData()
        {
        }

        public static PersonPrivateData Create(string firstName, string lastName)
        {
            var privateData = new PersonPrivateData();
            privateData.Apply(new PersonPrivateDataCreated(new PersonPrivateDataId().ToString(), firstName, lastName));
            return privateData;
        }

        public void ChangePersonAddress(string street, string country, string zipCode, string city)
        {
            Apply(new AddressChanged(city, country, zipCode, street));
        }

        public void On(PersonPrivateDataCreated @event)
        {
            Id = new PersonPrivateDataId(@event.PersonPrivateDataId);
            FirstName = @event.FirstName;
            LastName = @event.LastName;
        }

        public void On(AddressChanged @event)
        {
            PersonAddress = new Address()
            {
                City = @event.City,
                Country = @event.Country,
                Street = @event.Street,
                ZipCode = @event.ZipCode
            };
        }
    }
}
