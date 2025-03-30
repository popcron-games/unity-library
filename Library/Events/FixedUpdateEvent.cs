namespace UnityLibrary.Events
{
    public readonly struct FixedUpdateEvent
    {
        public readonly double delta;

        public readonly float DeltaAsFloat => (float)delta;

        public FixedUpdateEvent(double delta)
        {
            this.delta = delta;
        }
    }
}