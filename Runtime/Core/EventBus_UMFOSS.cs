using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// A lightweight, type-safe, static event bus for decoupled communication
    /// between game systems. Any system can publish or subscribe to events
    /// without holding direct references to other systems.
    ///
    /// Usage:
    ///   EventBus.Subscribe&lt;MyEvent&gt;(OnMyEvent);
    ///   EventBus.Publish(new MyEvent { ... });
    ///   EventBus.Unsubscribe&lt;MyEvent&gt;(OnMyEvent);
    ///
    /// Always unsubscribe in OnDisable/OnDestroy to prevent memory leaks.
    /// </summary>
    public static class EventBus
    {
        // Each event type T gets its own list of subscribers.
        // Using a static generic class avoids dictionary lookups entirely —
        // the CLR creates a separate static field per type argument.
        private static class EventChannel<T>
        {
            public static readonly List<Action<T>> Listeners = new List<Action<T>>();
        }

        /// <summary>
        /// Register a callback for events of type T.
        /// </summary>
        /// <typeparam name="T">Event struct type.</typeparam>
        /// <param name="listener">Callback to invoke when the event is published.</param>
        public static void Subscribe<T>(Action<T> listener)
        {
            if (listener == null) return;
            if (!EventChannel<T>.Listeners.Contains(listener))
            {
                EventChannel<T>.Listeners.Add(listener);
            }
        }

        /// <summary>
        /// Unregister a previously registered callback for events of type T.
        /// </summary>
        /// <typeparam name="T">Event struct type.</typeparam>
        /// <param name="listener">The callback to remove.</param>
        public static void Unsubscribe<T>(Action<T> listener)
        {
            if (listener == null) return;
            EventChannel<T>.Listeners.Remove(listener);
        }

        /// <summary>
        /// Publish an event of type T to all registered subscribers.
        /// Iterates in reverse so that listeners can safely unsubscribe
        /// during the callback without causing index errors.
        /// </summary>
        /// <typeparam name="T">Event struct type.</typeparam>
        /// <param name="eventData">The event data to broadcast.</param>
        public static void Publish<T>(T eventData)
        {
            var listeners = EventChannel<T>.Listeners;
            // Iterate in reverse so unsubscribing mid-iteration is safe
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i]?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Remove all subscribers for a specific event type.
        /// Useful for scene transitions or cleanup.
        /// </summary>
        /// <typeparam name="T">Event struct type to clear.</typeparam>
        public static void Clear<T>()
        {
            EventChannel<T>.Listeners.Clear();
        }
    }
}
