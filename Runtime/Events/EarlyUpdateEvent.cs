#nullable enable

namespace Popcron
{
    public readonly struct EarlyUpdateEvent : IEvent
    {
        public readonly float deltaTime;

        public EarlyUpdateEvent(float deltaTime)
        {
            this.deltaTime = deltaTime;
        }
    }
}