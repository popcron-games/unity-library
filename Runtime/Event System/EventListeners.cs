#nullable enable
using System;
using System.Collections.Generic;

namespace Popcron
{
    public static class EventListeners<T> where T : IEvent
    {
        public static readonly Queue<Action<T>> oneShotListeners = new Queue<Action<T>>();
        private static readonly List<Action<T>> listeners = new List<Action<T>>();

        public static IReadOnlyList<Action<T>> Listeners => listeners;

        public static bool Add<L>(L listener, Action<T> handler)
        {
            if (!listeners.Contains(handler))
            {
                listeners.Add(handler);
                StaticListeners<L>.handlersPerListeners[listener] = handler;
                StaticListeners<L>.listenerPerHandlers[handler] = listener;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Remove<L>(L listener)
        {
            if (StaticListeners<L>.handlersPerListeners.TryGetValue(listener, out Action<T> handler))
            {
                StaticListeners<L>.handlersPerListeners.Remove(listener);
                StaticListeners<L>.listenerPerHandlers.Remove(handler);
                listeners.Remove(handler);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void Add(Action<T> handler)
        {
            listeners.Add(handler);
        }

        public static void Remove(Action<T> handler)
        {
            listeners.Remove(handler);
        }

        public readonly struct StaticListeners<L>
        {
            public static readonly Dictionary<L, Action<T>> handlersPerListeners = new Dictionary<L, Action<T>>();
            public static readonly Dictionary<Action<T>, L> listenerPerHandlers = new Dictionary<Action<T>, L>();
        }
    }
}