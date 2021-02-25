using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tactical.DDD;

namespace Infrastructure.Repositories
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

            //foreach (Type type in
            //    Assembly.GetAssembly(eventBaseType).GetTypes()
            //    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(eventBaseType)))
            //{
            //    objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            //}
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
