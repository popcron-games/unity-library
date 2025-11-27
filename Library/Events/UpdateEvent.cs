namespace UnityLibrary.Events
{
    /// <summary>
    /// Called when unity updates.
    /// </summary>
    public readonly struct UpdateEvent
    {
        public readonly float delta;

        public UpdateEvent(float delta)
        {
            this.delta = delta;
        }
    }
}