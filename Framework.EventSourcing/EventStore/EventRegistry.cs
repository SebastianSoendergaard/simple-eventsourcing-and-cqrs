using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework.DDD.EventStore
{
    public class EventRegistry : IEventRegistry
    {
        private readonly Dictionary<string, Type> eventNameToType;

        public EventRegistry(Type eventBaseType)
        {
            var types = Assembly.GetAssembly(eventBaseType).GetTypes()
                                .Where(t => t.IsClass)
                                .Where(t => !t.IsAbstract)
                                .Where(t => t.IsSubclassOf(eventBaseType));

            eventNameToType = types.ToDictionary(t => t.Name);
        }

        public Type GetEventType(string eventName)
        {
            return eventNameToType[eventName];
        }

        public string GetEventName(IDomainEvent evt)
        {
            return evt.GetType().Name;
        }
    }
}
