namespace UnityLibrary.Events
{
    public readonly struct PreUpdateEvent
    {
        public readonly double delta;

        public readonly float DeltaAsFloat => (float)delta;

        public PreUpdateEvent(double delta)
        {
            this.delta = delta;
        }
    }
}