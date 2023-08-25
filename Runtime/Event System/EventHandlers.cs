using System.Collections.Generic;

namespace Popcron.Events
{
    public static class EventHandlers
    {
        private static readonly List<IEventHandler> handlers = new List<IEventHandler>();

        public static IReadOnlyList<IEventHandler> Handlers => handlers;

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