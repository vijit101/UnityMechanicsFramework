using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Global publish/subscribe event bus for decoupled communication between mechanics.
    /// Mechanics publish events here and subscribe to events they care about without
    /// holding any direct reference to each other.
    ///
    /// Usage:
    ///   EventBus.Subscribe&lt;PlayerJumpedEvent&gt;(OnPlayerJumped);
    ///   EventBus.Publish(new PlayerJumpedEvent { height = 12f });
    ///   EventBus.Unsubscribe&lt;PlayerJumpedEvent&gt;(OnPlayerJumped);
    ///
    /// Always unsubscribe in OnDisable/OnDestroy to avoid ghost listeners on destroyed objects.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> subscribers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Subscribe to an event of type T.
        /// The action is called every time T is published.
        /// </summary>
        public static void Subscribe<T>(Action<T> listener)
        {
            Type type = typeof(T);

            if (subscribers.ContainsKey(type))
                subscribers[type] = Delegate.Combine(subscribers[type], listener);
            else
                subscribers[type] = listener;
        }

        /// <summary>
        /// Unsubscribe a previously registered listener for event type T.
        /// Call this in OnDisable or OnDestroy to prevent null-reference callbacks.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> listener)
        {
            Type type = typeof(T);

            if (!subscribers.ContainsKey(type)) return;

            subscribers[type] = Delegate.Remove(subscribers[type], listener);

            if (subscribers[type] == null)
                subscribers.Remove(type);
        }

        /// <summary>
        /// Publish an event of type T to all current subscribers.
        /// </summary>
        public static void Publish<T>(T eventData)
        {
            Type type = typeof(T);

            if (subscribers.TryGetValue(type, out Delegate del))
                (del as Action<T>)?.Invoke(eventData);
        }
    }
}
