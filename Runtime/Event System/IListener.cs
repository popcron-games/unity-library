namespace Popcron
{
    /// <summary>
    /// Marks implementors in <see cref="Everything"/> as listeners for events of type <typeparamref name="T"/>.
    /// </summary>
    public interface IListener<T> where T : IEvent
    {
        void OnReceive(T ev);
    }
}