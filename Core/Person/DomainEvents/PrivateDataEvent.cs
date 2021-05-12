using Core.Common.DomainEvents;

namespace Core.Person.DomainEvents
{
    public class PrivateDataEvent : DomainEvent
    {
        public string PersonId { get; }

        public PrivateDataEvent(string personId)
        {
            PersonId = personId;
        }
    }
}
