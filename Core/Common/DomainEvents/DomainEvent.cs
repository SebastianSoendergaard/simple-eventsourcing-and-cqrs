using System;
using Framework.DDD;

namespace Core.Common.DomainEvents
{
    public class DomainEvent : IDomainEvent
    {

        public DomainEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public DateTime CreatedAt { get; set; }
    }
}
