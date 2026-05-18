using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>Static event bus for decoupled communication between systems.</summary>
    public static class EventBus_UMFOSS
    {
        private static readonly Dictionary<Type, List<Action<object>>> subscribers
            = new Dictionary<Type, List<Action<object>>>();
        private static readonly Dictionary<Delegate, Action<object>> callbackMap
            = new Dictionary<Delegate, Action<object>>();

        /// <summary>Subscribe to an event type. Callback fires whenever that event is published.</summary>
        public static void Subscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (!subscribers.ContainsKey(type))
                subscribers[type] = new List<Action<object>>();

            Action<object> wrapped = e => callback((T)e);
            callbackMap[callback] = wrapped;
            subscribers[type].Add(wrapped);
        }

        /// <summary>Unsubscribes a previously registered callback for an event type.</summary>
        public static void Unsubscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (!subscribers.ContainsKey(type)) return;
            if (!callbackMap.TryGetValue(callback, out var wrapped)) return;

            subscribers[type].Remove(wrapped);
            callbackMap.Remove(callback);
        }

        /// <summary>Publish an event. All subscribers for this type will be notified immediately.</summary>
        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (!subscribers.ContainsKey(type)) return;

            foreach (var handler in subscribers[type])
                handler(eventData);
        }

        /// <summary>Removes all subscribers. Call on scene unload to avoid stale references.</summary>
        public static void Clear()
        {
            subscribers.Clear();
            callbackMap.Clear();
        }
    }
}
