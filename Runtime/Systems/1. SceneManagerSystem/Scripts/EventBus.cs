using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Minimal static publish/subscribe event bus.
    /// Allows mechanics to communicate without holding direct references to each other.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> subscribers = new Dictionary<Type, Delegate>();

        /// <summary>Subscribe a handler to an event type.</summary>
        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (subscribers.TryGetValue(type, out var existing))
                subscribers[type] = Delegate.Combine(existing, handler);
            else
                subscribers[type] = handler;
        }

        /// <summary>Unsubscribe a handler from an event type.</summary>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!subscribers.TryGetValue(type, out var existing)) return;

            var updated = Delegate.Remove(existing, handler);
            if (updated == null)
                subscribers.Remove(type);
            else
                subscribers[type] = updated;
        }

        /// <summary>Publish an event to all subscribers of its type.</summary>
        public static void Publish<T>(T evt)
        {
            if (subscribers.TryGetValue(typeof(T), out var del))
                (del as Action<T>)?.Invoke(evt);
        }

        /// <summary>Removes every subscriber. Useful for tests and full resets.</summary>
        public static void ClearAll() => subscribers.Clear();
    }
}
