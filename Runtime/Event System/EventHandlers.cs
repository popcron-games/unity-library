using System.Collections.Generic;

namespace Popcron
{
    public static class EventHandlers
    {
        private static readonly HashSet<IEventHandler> handlers = new HashSet<IEventHandler>();

        public static ICollection<IEventHandler> Handlers => handlers;

        public static void Add<T>(T handler) where T : IEventHandler
        {
            handlers.Add(handler);
        }

        public static void Remove<T>(T handler) where T : IEventHandler
        {
            handlers.Remove(handler);
        }
    }
}