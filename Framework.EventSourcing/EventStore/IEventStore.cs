using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.DDD.EventStore
{
    public interface IEventStore
    {
        Task SaveAsync(IEntityId aggregateRootId, int originatingVersion, IReadOnlyCollection<IDomainEvent> events, string aggregateName = "Aggregate Name");

        Task DeleteAsync(IEntityId aggregateRootId);

        Task<IReadOnlyCollection<IDomainEvent>> LoadAsync(IEntityId aggregateRootId);
    }
}
