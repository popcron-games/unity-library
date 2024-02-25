namespace Game.Events
{
    public readonly struct LateUpdateEvent
    {
        public readonly float delta;

        public LateUpdateEvent(float delta)
        {
            this.delta = delta;
        }
    }
}