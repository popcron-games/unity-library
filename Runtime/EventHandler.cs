#nullable enable
using System;
using Popcron;

public readonly struct EventHandler<T> where T : IEvent
{
    private readonly Action<T>? action;

    public EventHandler(Action<T> action)
    {
        this.action = action;
    }

    public void Enable()
    {
        if (action != null)
        {
            EventListeners<T>.Add(action);
        }
    }

    public void Disable()
    {
        if (action != null)
        {
            EventListeners<T>.Remove(action);
        }
    }
}