using System;
using System.Collections.Generic;

public class EventManager : AddressableSingleton<EventManager>
{
    private readonly Dictionary<Type, List<Delegate>> m_events = new();

    public delegate void EventCallback<in T>(T data);

    public static void AddListener<T>(EventCallback<T> callback)
    {
        if (Instance.m_events.TryGetValue(typeof(T), out var listeners))
        {
            if (!listeners.Contains(callback))
            {
                listeners.Add(callback);
            }
        }
        else
        {
            listeners = new List<Delegate> { callback };
            Instance.m_events.Add(typeof(T), listeners);
        }
    }

    public static void RemoveListener<T>(EventCallback<T> callback)
    {
        if (!Instance.m_events.TryGetValue(typeof(T), out var listeners)) return;

        if (listeners.Contains(callback)) listeners.Remove(callback);
    }

    public static void RemoveListeners<T>()
    {
        if (!Instance.m_events.TryGetValue(typeof(T), out _)) return;

        Instance.m_events.Remove(typeof(T));
    }

    public static void Trigger<T>(T triggeredKey)
    {
        if (!Instance.m_events.TryGetValue(typeof(T), out var listeners))
        {
            return;
        }

        foreach (var listener in listeners.ToArray())
        {
            listener.DynamicInvoke(triggeredKey);
        }
    }

    protected override void FirstLoad()
    {
        m_events.Clear();
    }
}   