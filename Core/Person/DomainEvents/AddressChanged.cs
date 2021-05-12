using Core.Common.DomainEvents;

namespace Core.Person.DomainEvents
{
    public class AddressChanged : DomainEvent
    {
        public string PersonId { get; }
        public string City { get;  }
        public string Country { get;  }
        public string ZipCode { get;  }
        public string Street { get;  }

        public AddressChanged(string personId, string city, string country, string zipcode, string street)
        {
            PersonId = personId;
            City = city;
            Country = country;
            ZipCode = zipcode;
            Street = street;
        }
    }
}

