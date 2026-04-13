using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Lightweight publish/subscribe bus for decoupled cross-system events.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            var type = typeof(T);
            if (!Subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                Subscribers[type] = list;
            }

            if (!list.Contains(handler))
            {
                list.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            var type = typeof(T);
            if (!Subscribers.TryGetValue(type, out var list))
            {
                return;
            }

            list.Remove(handler);
            if (list.Count == 0)
            {
                Subscribers.Remove(type);
            }
        }

        public static void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (!Subscribers.TryGetValue(type, out var list))
            {
                return;
            }

            // Copy to avoid re-entrancy issues if handlers subscribe during publish
            var snapshot = list.ToArray();
            foreach (var d in snapshot)
            {
                if (d is Action<T> action)
                {
                    action.Invoke(evt);
                }
            }
        }

        public static void ClearAll()
        {
            Subscribers.Clear();
        }
    }
}
