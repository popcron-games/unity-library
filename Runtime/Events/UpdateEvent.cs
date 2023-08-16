using Popcron;

namespace UnityEngine
{
    /// <summary>
    /// Gets called only when the game is running.
    /// </summary>
    public readonly struct UpdateEvent : IEvent
    {
        public readonly float deltaTime;

        public UpdateEvent(float deltaTime)
        {
            this.deltaTime = deltaTime;
        }
    }
}