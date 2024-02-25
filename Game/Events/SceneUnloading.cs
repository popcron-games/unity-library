namespace UnityLibrary.Events
{
    /// <summary>
    /// Occurs before a scene is unloaded using <see cref="Functions.UnloadScene"/>.
    /// </summary>
    public readonly struct SceneUnloading
    {
        public readonly string scene;

        public SceneUnloading(string scene)
        {
            this.scene = scene;
        }
    }
}