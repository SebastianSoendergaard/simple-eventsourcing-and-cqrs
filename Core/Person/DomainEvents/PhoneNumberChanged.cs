namespace Core.Person.DomainEvents
{
    public class PhoneNumberChanged : PrivateDataEvent
    {
        public string PhoneNumber { get;  }

        public PhoneNumberChanged(string personId, string phoneNumber)
            : base(personId)
        {
            PhoneNumber = phoneNumber;
        }
    }
}

