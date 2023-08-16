#nullable enable
using Popcron;
using Popcron.Events;
using System;

public static class EventExtensions
{
    /// <summary>
    /// Dispatches this event to all <see cref="IListener{T}"/> instances in <see cref="Everything"/>
    /// to any <see cref="Action{T}"/> instances in <see cref="EventListeners{T}"/>,
    /// to all <see cref="IEventHandler"/> instances in <see cref="EventHandlers"/> (entire events, not filtered)
    /// </summary>
    public static T Dispatch<T>(this T e) where T : IEvent
    {
        foreach (IListener<T> listener in Everything.GetAllThatAre<IListener<T>>())
        {
            listener.OnEvent(e);
        }

        foreach (IEventHandler handler in Everything.GetAllThatAre<IEventHandler>())
        {
            handler.Dispatch(e);
        }

        foreach (Action<T> listener in EventListeners<T>.Listeners)
        {
            listener.Invoke(e);
        }

        while (EventListeners<T>.oneShotListeners.Count > 0)
        {
            EventListeners<T>.oneShotListeners.Dequeue().Invoke(e);
        }

        foreach (IEventHandler handler in EventHandlers.Handlers)
        {
            handler.Dispatch(e);
        }

        return e;
    }

    public static void AddListener<L, T>(this T e, L listener, Action<T> action) where T : IEvent
    {
        EventListeners<T>.Add(listener, action);
    }

    public static void RemoveListener<L, T>(this T e, L listener) where T : IEvent
    {
        EventListeners<T>.Remove(listener);
    }

    public static void AddListener<T>(this T e, Action<T> action) where T : IEvent
    {
        EventListeners<T>.Add(action);
    }

    public static void RemoveListener<T>(this T e, Action<T> action) where T : IEvent
    {
        EventListeners<T>.Remove(action);
    }

    public static void AddListenerOneShot<T>(this T e, Action<T> action) where T : IEvent
    {
        EventListeners<T>.oneShotListeners.Enqueue(action);
    }
}
