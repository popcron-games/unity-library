using Popcron.Events;

namespace Popcron
{
    /// <summary>
    /// Marks implementors in <see cref="Everything"/> and <see cref="EventHandlers"> as receivers of any event.
    /// </summary>
    public interface IEventHandler
    {
        void Dispatch<T>(T e) where T : IEvent;
    }
}