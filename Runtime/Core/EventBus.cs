using System;
using System.Collections.Generic;

/// <summary>
/// Decoupled event communication system. Mechanics publish events without
/// knowing who listens. Subscribers react without knowing who publishes.
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> subscribers = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (!subscribers.ContainsKey(type))
            subscribers[type] = new List<Delegate>();
        subscribers[type].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (subscribers.ContainsKey(type))
            subscribers[type].Remove(handler);
    }

    public static void Publish<T>(T eventData)
    {
        var type = typeof(T);
        if (!subscribers.ContainsKey(type)) return;
        for (int i = subscribers[type].Count - 1; i >= 0; i--)
        {
            if (subscribers[type][i] is Action<T> action)
                action.Invoke(eventData);
        }
    }

    public static void Clear()
    {
        subscribers.Clear();
    }
}
