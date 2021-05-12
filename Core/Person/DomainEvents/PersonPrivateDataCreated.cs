using Core.Common.DomainEvents;

namespace Core.Person.DomainEvents
{
    public class PersonPrivateDataCreated : DomainEvent
    {
        public string PersonPrivateDataId { get; }
        public string PersonId { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public PersonPrivateDataCreated(string personPrivateDataId, string personId, string firstName, string lastName)
        {
            PersonPrivateDataId = personPrivateDataId;
            PersonId = personId;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
