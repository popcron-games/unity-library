#nullable enable

namespace Popcron
{
    public readonly struct FixedUpdateEvent : IEvent
    {
        public readonly float deltaTime;

        public FixedUpdateEvent(float deltaTime)
        {
            this.deltaTime = deltaTime;
        }
    }
}