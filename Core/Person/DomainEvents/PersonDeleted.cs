namespace Core.Person.DomainEvents
{
    public class PersonDeleted : DomainEvent
    {
        public string Reason { get; }

        public PersonDeleted(string reason)
        {
            Reason = reason;
        }


    }
}
