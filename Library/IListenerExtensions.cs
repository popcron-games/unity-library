#nullable enable
namespace Library
{
    public static class IListenerExtensions
    {
        public static void Tell<T, E>(this T listener, VirtualMachine vm, E e)
        {
            if (listener is IListener<E> typedListener)
            {
                typedListener.Receive(vm, e);
            }
        }
    }
}