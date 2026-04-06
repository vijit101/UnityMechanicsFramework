using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// A lightweight, static, type-safe event bus for decoupled communication
    /// between gameplay systems. Any system can publish an event, and any other
    /// system can subscribe — without either side knowing the other exists.
    ///
    /// Usage:
    ///   EventBus.Subscribe&lt;PlayerJumpedEvent&gt;(OnPlayerJumped);
    ///   EventBus.Publish(new PlayerJumpedEvent { height = 12f });
    ///   EventBus.Unsubscribe&lt;PlayerJumpedEvent&gt;(OnPlayerJumped);
    /// </summary>
    public static class EventBus
    {
        // ──────────────────────────────────────────────
        // Internal storage — one list of callbacks per event type
        // ──────────────────────────────────────────────

        private static readonly Dictionary<Type, List<Delegate>> subscribers
            = new Dictionary<Type, List<Delegate>>();

        // ──────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────

        /// <summary>
        /// Register a callback that will be invoked whenever an event of type T is published.
        /// </summary>
        /// <typeparam name="T">The event struct or class type.</typeparam>
        /// <param name="callback">The method to call when the event fires.</param>
        public static void Subscribe<T>(Action<T> callback)
        {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType))
            {
                subscribers[eventType] = new List<Delegate>();
            }

            subscribers[eventType].Add(callback);
        }

        /// <summary>
        /// Remove a previously registered callback so it no longer receives events of type T.
        /// Always call this in OnDisable or OnDestroy to prevent memory leaks.
        /// </summary>
        /// <typeparam name="T">The event struct or class type.</typeparam>
        /// <param name="callback">The exact method reference that was passed to Subscribe.</param>
        public static void Unsubscribe<T>(Action<T> callback)
        {
            Type eventType = typeof(T);

            if (subscribers.ContainsKey(eventType))
            {
                subscribers[eventType].Remove(callback);
            }
        }

        /// <summary>
        /// Publish an event to all subscribers of type T.
        /// All registered callbacks are invoked synchronously in registration order.
        /// </summary>
        /// <typeparam name="T">The event struct or class type.</typeparam>
        /// <param name="eventData">The event instance containing the payload data.</param>
        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType))
            {
                return;
            }

            // Iterate over a copy to allow subscribers to unsubscribe during handling
            List<Delegate> subscriberList = new List<Delegate>(subscribers[eventType]);

            foreach (Delegate subscriber in subscriberList)
            {
                if (subscriber is Action<T> typedCallback)
                {
                    typedCallback.Invoke(eventData);
                }
            }
        }

        /// <summary>
        /// Remove all subscribers for all event types.
        /// Useful for scene transitions or test teardown.
        /// </summary>
        public static void ClearAll()
        {
            subscribers.Clear();
        }

        /// <summary>
        /// Remove all subscribers for a specific event type.
        /// </summary>
        /// <typeparam name="T">The event struct or class type to clear.</typeparam>
        public static void Clear<T>()
        {
            Type eventType = typeof(T);

            if (subscribers.ContainsKey(eventType))
            {
                subscribers[eventType].Clear();
            }
        }
    }
}
