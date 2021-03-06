﻿using Core.Common.DomainEvents;

namespace Core.Person.DomainEvents
{
    public class PersonCreated : DomainEvent
    {
        public string PersonId { get; }

        public PersonCreated(string personId)
        {
            PersonId = personId;
        }
    }
}
