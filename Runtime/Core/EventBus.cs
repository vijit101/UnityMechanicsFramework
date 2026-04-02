using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Simple type-based event bus for decoupled communication between systems.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> listeners = new Dictionary<Type, Delegate>();

        /// <summary>Subscribe a callback to events of type T.</summary>
        public static void Subscribe<T>(Action<T> callback) where T : struct
        {
            Type eventType = typeof(T);

            if (listeners.TryGetValue(eventType, out Delegate existing))
            {
                listeners[eventType] = Delegate.Combine(existing, callback);
            }
            else
            {
                listeners[eventType] = callback;
            }
        }

        /// <summary>Unsubscribe a callback. Safe to call even if never subscribed.</summary>
        public static void Unsubscribe<T>(Action<T> callback) where T : struct
        {
            Type eventType = typeof(T);

            if (listeners.TryGetValue(eventType, out Delegate existing))
            {
                Delegate result = Delegate.Remove(existing, callback);

                if (result == null)
                {
                    listeners.Remove(eventType);
                }
                else
                {
                    listeners[eventType] = result;
                }
            }
        }

        /// <summary>Publish an event to all subscribers of type T (invoked synchronously).</summary>
        public static void Publish<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);

            if (listeners.TryGetValue(eventType, out Delegate existing))
            {
                ((Action<T>)existing).Invoke(eventData);
            }
        }

        /// <summary>Remove all subscribers. Call on scene cleanup or test teardown.</summary>
        public static void Clear()
        {
            listeners.Clear();
        }
    }
}
