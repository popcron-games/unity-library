namespace UnityLibrary.Events
{
    public readonly struct LateUpdateEvent
    {
        public readonly double delta;

        public readonly float DeltaAsFloat => (float)delta;

        public LateUpdateEvent(double delta)
        {
            this.delta = delta;
        }
    }
}