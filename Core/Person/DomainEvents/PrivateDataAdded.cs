using Core.Common.DomainEvents;

namespace Core.Person.DomainEvents
{
    public class PrivateDataAdded : DomainEvent
    {
        public string PrivateDataId { get; }

        public PrivateDataAdded(string privateDataId)
        {
            PrivateDataId = privateDataId;
        }
    }
}
