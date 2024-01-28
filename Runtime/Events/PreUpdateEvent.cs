namespace Library.Events
{
    public readonly struct PreUpdateEvent
    {
        public readonly float delta;

        public PreUpdateEvent(float delta)
        {
            this.delta = delta;
        }
    }
}