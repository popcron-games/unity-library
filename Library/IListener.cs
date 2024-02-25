#nullable enable
namespace Game
{
    /// <summary>
    /// Systems implementing this interface will receive events when using <see cref="VirtualMachine.Broadcast{T}"/>
    /// </summary>
    public interface IListener<T> where T : notnull
    {
        void Receive(VirtualMachine vm, ref T e);
    }
}