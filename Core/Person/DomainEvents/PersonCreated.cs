namespace Core.Person.DomainEvents
{
    public class PersonCreated : DomainEvent
    {
        public string PersonId { get; }
        public string PersonPrivateDataId { get; }

        public PersonCreated(string personId, string personPrivateDataId)
        {
            PersonId = personId;
            PersonPrivateDataId = personPrivateDataId;
        }
    }
}
