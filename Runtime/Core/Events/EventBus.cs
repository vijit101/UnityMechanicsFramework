using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// A lightweight, type-safe publish/subscribe event bus.
    /// Events are structs identified by their type. Any script can publish or subscribe
    /// without holding a direct reference to the publisher.
    /// </summary>
    public static class EventBus
    {
        /// <summary>
        /// Typed channel that holds subscribers for a specific event type.
        /// Using a nested generic class causes the CLR to create a separate static
        /// field per type T, giving us a type-safe dictionary without casts.
        /// </summary>
        private static class Channel<T> where T : struct
        {
            public static readonly List<Action<T>> Listeners = new List<Action<T>>();
        }

        /// <summary>
        /// Subscribes a listener to events of type T.
        /// </summary>
        public static void Subscribe<T>(Action<T> listener) where T : struct
        {
            if (listener == null) return;
            Channel<T>.Listeners.Add(listener);
        }

        /// <summary>
        /// Unsubscribes a listener from events of type T.
        /// Always unsubscribe in OnDisable/OnDestroy to prevent memory leaks.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            Channel<T>.Listeners.Remove(listener);
        }

        /// <summary>
        /// Publishes an event to all subscribers of type T.
        /// Iterates in reverse to safely handle unsubscriptions during dispatch.
        /// </summary>
        public static void Publish<T>(T eventData) where T : struct
        {
            var listeners = Channel<T>.Listeners;
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].Invoke(eventData);
            }
        }

        /// <summary>
        /// Removes all subscribers for event type T.
        /// Useful for scene cleanup or testing.
        /// </summary>
        public static void Clear<T>() where T : struct
        {
            Channel<T>.Listeners.Clear();
        }
    }
}
