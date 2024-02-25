namespace Game.Events
{
    public readonly struct UpdateEvent
    {
        public readonly float delta;

        public UpdateEvent(float delta)
        {
            this.delta = delta;
        }
    }
}