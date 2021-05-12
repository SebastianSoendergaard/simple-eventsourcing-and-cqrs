namespace Core.Person.DomainEvents
{
    public class AddressChanged : PrivateDataEvent
    {
        public string City { get;  }
        public string Country { get;  }
        public string ZipCode { get;  }
        public string Street { get;  }

        public AddressChanged(string personId, string city, string country, string zipcode, string street)
            : base(personId)
        {
            City = city;
            Country = country;
            ZipCode = zipcode;
            Street = street;
        }
    }
}

