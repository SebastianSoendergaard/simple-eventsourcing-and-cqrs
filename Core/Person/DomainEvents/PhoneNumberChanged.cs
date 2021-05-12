using Core.Common.DomainEvents;

namespace Core.Person.DomainEvents
{
    public class PhoneNumberChanged : DomainEvent
    {
        public string PersonId { get; }
        public string PhoneNumber { get;  }

        public PhoneNumberChanged(string personId, string phoneNumber)
        {
            PersonId = personId;
            PhoneNumber = phoneNumber;
        }
    }
}

