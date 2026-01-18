#nullable enable
using UnityLibrary;

public static class EventExtensions
{
    /// <summary>
    /// Broadcasts the given <paramref name="ev"/> to all <see cref="IListener{T}"/> instances
    /// that can be found through <see cref="UnityApplication.VM"/>.
    /// </summary>
    public static void Broadcast<T>(this T ev) where T : notnull
    {
        UnityApplication.VM.Broadcast(ev);
    }
}