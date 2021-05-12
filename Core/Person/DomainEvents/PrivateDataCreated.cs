namespace Core.Person.DomainEvents
{
    public class PrivateDataCreated : PrivateDataEvent
    {
        public string PersonPrivateDataId { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public PrivateDataCreated(string personId, string personPrivateDataId, string firstName, string lastName)
            : base(personId)
        {
            PersonPrivateDataId = personPrivateDataId;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
