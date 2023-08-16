using Popcron;

namespace UnityEditor
{
    /// <summary>
    /// Gets called before the play button is pressed, but after editor transitions to playing.
    /// Editor only.
    /// </summary>
    public readonly struct AboutToStartPlaying : IEvent
    {

    }
}