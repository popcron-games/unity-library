using UnityEngine.SceneManagement;

namespace Library.Events
{
    /// <summary>
    /// Occurs after a scene is unloaded using <see cref="Functions.UnloadScene"/>.
    /// </summary>
    public readonly struct SceneUnloaded
    {
        public readonly Scene scene;

        public SceneUnloaded(Scene scene)
        {
            this.scene = scene;
        }
    }
}