#nullable enable
namespace Library
{
    /// <summary>
    /// Systems implementing this interface will receive events when using <see cref="VirtualMachine.Broadcast{T}"/>
    /// </summary>
    public interface IListener<T>
    {
        void Receive(VirtualMachine vm, T e);
    }
}