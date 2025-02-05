#nullable enable
namespace UnityLibrary
{
    /// <summary>
    /// Systems that implement this interface will receive all events broadcast.
    /// Useful in case where the implementer may contain more objects to dispatch
    /// inside of it, to dispatch the event a layer further in the object hierarchy.
    /// </summary>
    public interface IAnyListener
    {
        void Receive<T>(VirtualMachine vm, ref T e) where T : notnull;
    }
}