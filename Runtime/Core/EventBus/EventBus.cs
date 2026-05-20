using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// A lightweight, type-safe publish/subscribe event bus for decoupled communication
    /// between gameplay mechanics. Any system can publish or subscribe to events without
    /// holding direct references to other systems.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> subscribers =
            new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribe a callback to a specific event type.
        /// The callback will be invoked every time an event of type T is published.
        /// </summary>
        /// <typeparam name="T">The event type to listen for.</typeparam>
        /// <param name="callback">The action to invoke when the event is published.</param>
        public static void Subscribe<T>(Action<T> callback)
        {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType))
            {
                subscribers[eventType] = new List<Delegate>();
            }

            if (!subscribers[eventType].Contains(callback))
            {
                subscribers[eventType].Add(callback);
            }
        }

        /// <summary>
        /// Unsubscribe a previously registered callback from a specific event type.
        /// Always unsubscribe in OnDisable or OnDestroy to prevent memory leaks.
        /// </summary>
        /// <typeparam name="T">The event type to stop listening for.</typeparam>
        /// <param name="callback">The action to remove.</param>
        public static void Unsubscribe<T>(Action<T> callback)
        {
            Type eventType = typeof(T);

            if (subscribers.ContainsKey(eventType))
            {
                subscribers[eventType].Remove(callback);

                if (subscribers[eventType].Count == 0)
                {
                    subscribers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Publish an event to all subscribers of type T.
        /// Subscribers are invoked synchronously in the order they were registered.
        /// </summary>
        /// <typeparam name="T">The event type to publish.</typeparam>
        /// <param name="eventData">The event data payload.</param>
        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType))
            {
                return;
            }

            // Iterate over a copy to allow subscribers to unsubscribe during callback
            List<Delegate> subscriberList = new List<Delegate>(subscribers[eventType]);

            for (int i = 0; i < subscriberList.Count; i++)
            {
                Action<T> callback = subscriberList[i] as Action<T>;
                callback?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Remove all subscribers for all event types.
        /// Useful for scene transitions or test teardown.
        /// </summary>
        public static void Clear()
        {
            subscribers.Clear();
        }
    }
}
