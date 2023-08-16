#nullable enable
using Popcron;

namespace UnityEngine
{
    public readonly struct GUIEvent : IEvent
    {
        public readonly Event current;

        public GUIEvent(Event current)
        {
            this.current = current;
        }
    }
}