using UnityEngine.ResourceManagement.ResourceProviders;

namespace Library.Events
{
    /// <summary>
    /// Occurs after a scene is loaded using <see cref="Functions.LoadScene"/>.
    /// </summary>
    public readonly struct SceneLoaded
    {
        public readonly SceneInstance scene;

        public SceneLoaded(SceneInstance scene)
        {
            this.scene = scene;
        }
    }
}