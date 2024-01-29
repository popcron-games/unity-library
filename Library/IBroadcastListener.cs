#nullable enable
namespace Library
{
    /// <summary>
    /// Systems that implement this interface will receive all broadcast events (intended for broadcasting further).
    /// </summary>
    public interface IBroadcastListener
    {
        void Receive<T>(VirtualMachine vm, T e) where T : notnull;
    }
}