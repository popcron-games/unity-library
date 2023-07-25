namespace Popcron
{
    /// <summary>
    /// Marks implementors in <see cref="Everything"/> as listeners for events of type <typeparamref name="T"/>.
    /// </summary>
    public interface IListener<in T> where T : IEvent
    {
        void OnEvent(T e);
    }
}