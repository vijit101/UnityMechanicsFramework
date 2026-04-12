using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    public static class EventBus
    {
        private static Dictionary<Type, Action<object>> _events = new();

        public static void Subscribe<T>(Action<T> listener)
        {
            Type type = typeof(T);

            if (!_events.ContainsKey(type))
                _events[type] = delegate { };

            _events[type] += (e) => listener((T)e);
        }

        public static void Publish<T>(T evt)
        {
            Type type = typeof(T);

            if (_events.ContainsKey(type))
                _events[type].Invoke(evt);
        }
    }
}