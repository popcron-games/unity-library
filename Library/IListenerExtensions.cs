#nullable enable
namespace Game
{
    public static class IListenerExtensions
    {
        /// <summary>
        /// Invokes <see cref="IListener{T}.Receive(VirtualMachine, ref T)"/>, assuming
        /// the given <paramref name="ev"/> input parameter is a listener of such events.
        /// </summary>
        public static void Tell<L, T>(this L listener, VirtualMachine vm, ref T ev) where T : notnull
        {
            if (listener is IListener<T> typedListener)
            {
                typedListener.Receive(vm, ref ev);
            }
        }
    }
}