using System.Collections.Generic;
using Core.Person.DomainEvents;
using Framework.DDD;

namespace Core.Person
{
    public class PersonPrivateData : Framework.DDD.EventStore.AggregateRoot<PersonPrivateDataId>
    {
        public override PersonPrivateDataId Id { get; protected set; }
        public PersonId PersonId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Address PersonAddress { get; private set; }
        public string PhoneNumber { get; private set; }

        public PersonPrivateData(IEnumerable<IDomainEvent> events) : base(events)
        {
        }

        private PersonPrivateData()
        {
        }

        public static PersonPrivateData Create(PersonId personId, string firstName, string lastName)
        {
            var privateData = new PersonPrivateData();
            privateData.Apply(new PersonPrivateDataCreated(new PersonPrivateDataId().ToString(), personId.ToString(), firstName, lastName));
            return privateData;
        }

        public void PrepareDelete()
        {
            ChangePersonAddress("", "", "", "");
            ChangePhoneNumber("");
        }

        public void ChangePersonAddress(string street, string country, string zipCode, string city)
        {
            Apply(new AddressChanged(PersonId.ToString(), city, country, zipCode, street));
        }

        public void ChangePhoneNumber(string phoneNumber)
        {
            Apply(new PhoneNumberChanged(PersonId.ToString(), phoneNumber));
        }

        public void On(PersonPrivateDataCreated evt)
        {
            Id = new PersonPrivateDataId(evt.PersonPrivateDataId);
            PersonId = new PersonId(evt.PersonId);
            FirstName = evt.FirstName;
            LastName = evt.LastName;
        }

        public void On(AddressChanged evt)
        {
            PersonAddress = new Address()
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
