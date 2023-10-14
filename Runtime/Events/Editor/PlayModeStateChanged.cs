using Popcron;

namespace UnityEditor
{
    /// <summary>
    /// Occurs when EditorApplication.playModeStateChanged gets called.
    /// <para></para>
    /// Will occur in builds as well but only for <see cref="PlayModeStateChange.EnteredPlayMode"/> and <see cref="PlayModeStateChange.ExitingPlayMode"/> options.
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