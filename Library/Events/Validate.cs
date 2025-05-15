namespace UnityLibrary.Events
{
    /// <summary>
    /// Triggered to check if <see cref="VirtualMachine"/> is able to execute
    /// from initial conditions without any issues that would cause an exception at runtime.
    /// <para></para>
    /// The <see cref="IListener{}"/>s of this event should throw exceptions to indicate a failure issue when
    /// assertions fail (such as references missing when they are expected to never be <see cref="null"/>)
    /// </summary>
    public struct Validate
    {
        public bool failed;
    }
}