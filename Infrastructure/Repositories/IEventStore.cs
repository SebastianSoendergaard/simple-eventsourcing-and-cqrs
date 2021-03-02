using System.Collections.Generic;
using System.Threading.Tasks;
using Tactical.DDD;

namespace Infrastructure.Repositories
{
    public interface IEventStore
    {
        Task SaveAsync(IEntityId aggregateRootId, int originatingVersion, IReadOnlyCollection<IDomainEvent> events, string aggregateName = "Aggregate Name");

        Task DeleteAsync(IEntityId aggregateRootId);

        Task<IReadOnlyCollection<IDomainEvent>> LoadAsync(IEntityId aggregateRootId);

        Task<IReadOnlyCollection<(IDomainEvent Event, int Index)>> GetAllEventsAsync(int startIndex, int max);
        Task<IReadOnlyCollection<(IDomainEvent Event, int Index)>> GetEventsByAggregateNamesAsync(int startIndex, int max, params string[] aggregateNames);
        Task<IReadOnlyCollection<(IDomainEvent Event, int Index)>> GetEventsByEventNamesAsync(int startIndex, int max, params string[] eventNames);
    }
}
