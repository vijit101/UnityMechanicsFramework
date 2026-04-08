using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> subscribers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> callback)
        {
            Type type = typeof(T);
            if (subscribers.TryGetValue(type, out Delegate existing))
            {
                subscribers[type] = (Action<T>)existing + callback;
                return;
            }

            subscribers[type] = callback;
        }

        public static void Unsubscribe<T>(Action<T> callback)
        {
            Type type = typeof(T);
            if (!subscribers.TryGetValue(type, out Delegate existing))
            {
                return;
            }

            Action<T> updated = (Action<T>)existing - callback;
            if (updated == null)
            {
                subscribers.Remove(type);
                return;
            }

            subscribers[type] = updated;
        }

        public static void Publish<T>(T eventData)
        {
            Type type = typeof(T);
            if (!subscribers.TryGetValue(type, out Delegate existing))
            {
                return;
            }

            (existing as Action<T>)?.Invoke(eventData);
        }
    }
}
