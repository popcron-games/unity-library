namespace UnityLibrary.Events
{
    /// <summary>
    /// Triggered when the physics system updates. By default this is after
    /// <see cref="FixedUpdateEvent"/>, but can be after <see cref="UpdateEvent"/> or manually
    /// as set in Project settings/Physics/Simulation mode.
    /// </summary>
    public readonly struct PhysicsUpdateEvent
    {
        public readonly float delta;

        public PhysicsUpdateEvent(float delta)
        {
            this.delta = delta;
        }
    }
}