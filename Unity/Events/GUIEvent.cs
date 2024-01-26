using UnityEngine;

namespace Library.Events
{
    public readonly struct GUIEvent
    {
        public readonly Event guiEvent;

        public GUIEvent(Event guiEvent)
        {
            this.guiEvent = guiEvent;
        }
    }
}