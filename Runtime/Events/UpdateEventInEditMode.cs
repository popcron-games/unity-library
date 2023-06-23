#nullable enable

namespace Popcron
{
    /// <summary>
    /// Gets called only when the update event happens in editor and when not playing.
    /// </summary>
    public readonly struct UpdateEventInEditMode : IEvent
    {
        public readonly float deltaTime;

        public UpdateEventInEditMode(float deltaTime)
        {
            this.deltaTime = deltaTime;
        }
    }
}