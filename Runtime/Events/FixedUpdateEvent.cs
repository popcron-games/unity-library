namespace Library.Events
{
    public readonly struct FixedUpdateEvent
    {
        public readonly float delta;

        public FixedUpdateEvent(float delta)
        {
            this.delta = delta;
        }
    }
}