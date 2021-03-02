namespace Core.Person.DomainEvents
{
    public class PersonPrivateDataCreated : DomainEvent
    {
        public string PersonPrivateDataId { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public PersonPrivateDataCreated(string personPrivateDataId, string firstName, string lastName)
        {
            PersonPrivateDataId = personPrivateDataId;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
