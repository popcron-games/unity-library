namespace UnityLibrary.Events
{
    /// <summary>
    /// Called when unity updates.
    /// </summary>
    public readonly struct UpdateEvent
    {
        public readonly double delta;

        public readonly float DeltaAsFloat => (float)delta;

        public UpdateEvent(double delta)
        {
            this.delta = delta;
        }
    }
}