// Author: Aditya Jaiswal, Atharv S. Jain
using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Lightweight type-keyed event bus for decoupled runtime messaging.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> subscribers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Subscribes a handler to events of type T.
        /// </summary>
        /// <typeparam name="T">The event data type.</typeparam>
        /// <param name="handler">The callback to invoke when the event is published.</param>
        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (!subscribers.TryGetValue(type, out var existingDelegate))
            {
                subscribers[type] = handler;
                return;
            }

            subscribers[type] = Delegate.Combine(existingDelegate, handler);
        }

        /// <summary>
        /// Unsubscribes a handler from events of type T.
        /// </summary>
        /// <typeparam name="T">The event data type.</typeparam>
        /// <param name="handler">The callback to remove from the event chain.</param>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (!subscribers.TryGetValue(type, out var existingDelegate))
            {
                return;
            }

            var updatedDelegate = Delegate.Remove(existingDelegate, handler);

            if (updatedDelegate == null)
            {
                subscribers.Remove(type);
                return;
            }

            subscribers[type] = updatedDelegate;
        }

        /// <summary>
        /// Publishes an event of type T to all subscribers.
        /// </summary>
        /// <typeparam name="T">The event data type.</typeparam>
        /// <param name="eventData">The payload to pass to subscribers.</param>
        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);

            if (!subscribers.TryGetValue(type, out var existingDelegate))
            {
                return;
            }

            if (existingDelegate is Action<T> callback)
            {
                callback.Invoke(eventData);
            }
        }
    }
}
