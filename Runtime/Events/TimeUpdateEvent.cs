#nullable enable

using Popcron;

namespace Popcron.Lib
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