using System.Collections.Generic;
using Core.Person.DomainEvents;
using Framework.DDD;

namespace Core.Person
{
    public class PrivateData : Framework.DDD.EventStore.AggregateRoot<PrivateDataId>
    {
        public override PrivateDataId Id { get; protected set; }
        public PersonId PersonId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Address Address { get; private set; }
        public string PhoneNumber { get; private set; }

        public PrivateData(IEnumerable<IDomainEvent> events) : base(events)
        {
        }

        private PrivateData()
        {
        }

        public static PrivateData Create(PersonId personId, string firstName, string lastName)
        {
            var privateData = new PrivateData();
            privateData.Apply(new PrivateDataCreated(personId.ToString(), new PrivateDataId().ToString(), firstName, lastName));
            return privateData;
        }

        public void ChangePersonAddress(string street, string country, string zipCode, string city)
        {
            Apply(new AddressChanged(PersonId.ToString(), city, country, zipCode, street));
        }

        public void ChangePhoneNumber(string phoneNumber)
        {
            Apply(new PhoneNumberChanged(PersonId.ToString(), phoneNumber));
        }

        public void On(PrivateDataCreated evt)
        {
            Id = new PrivateDataId(evt.PersonPrivateDataId);
            PersonId = new PersonId(evt.PersonId);
            FirstName = evt.FirstName;
            LastName = evt.LastName;
        }

        public void On(AddressChanged evt)
        {
            Address = new Address()
            {
                City = evt.City,
                Country = evt.Country,
                Street = evt.Street,
                ZipCode = evt.ZipCode
            };
        }

        public void On(PhoneNumberChanged evt)
        {
            PhoneNumber = evt.PhoneNumber;
        }
    }
}
