using Popcron;

namespace UnityEditor
{
    /// <summary>
    /// Occurs when EditorApplication.playModeStateChanged gets called.
    /// </summary>
    public readonly struct PlayModeStateChanged : IEvent
    {
        public readonly int value;

        public PlayModeStateChanged(int value)
        {
            this.value = value;
        }
    }
}