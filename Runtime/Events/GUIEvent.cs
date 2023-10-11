#nullable enable
using Popcron;

namespace UnityEngine
{
    /// <summary>
    /// When a singleton's <see cref="MonoBehaviour.OnGUI()"/> is called, this event is fired.
    /// </summary>
    public readonly struct GUIEvent : IEvent
    {
        public readonly Event current;

        public GUIEvent(Event current)
        {
            this.current = current;
        }
    }
}