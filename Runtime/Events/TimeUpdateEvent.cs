#nullable enable

namespace Popcron
{
    public readonly struct TimeUpdateEvent : IEvent
    {
        public readonly float deltaTime;

        public TimeUpdateEvent(float deltaTime)
        {
            this.deltaTime = deltaTime;
        }
    }
}