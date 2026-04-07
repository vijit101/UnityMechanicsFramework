using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Lightweight publish/subscribe hub for struct-based events.
    /// </summary>
    public static class EventBus
    {
        static readonly Dictionary<Type, Delegate> Subscribers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;
            var key = typeof(T);
            if (Subscribers.TryGetValue(key, out var existing))
                Subscribers[key] = Delegate.Combine(existing, handler);
            else
                Subscribers[key] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;
            var key = typeof(T);
            if (!Subscribers.TryGetValue(key, out var existing)) return;
            var result = Delegate.Remove(existing, handler);
            if (result == null)
                Subscribers.Remove(key);
            else
                Subscribers[key] = result;
        }

        public static void Publish<T>(T evt) where T : struct
        {
            if (!Subscribers.TryGetValue(typeof(T), out var del)) return;
            ((Action<T>)del)?.Invoke(evt);
        }
    }
}
