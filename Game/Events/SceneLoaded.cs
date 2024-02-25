namespace UnityLibrary.Events
{
    /// <summary>
    /// Occurs after a scene is loaded using <see cref="Functions.LoadScene"/>.
    /// </summary>
    public readonly struct SceneLoaded
    {
        public readonly string sceneName;

        public SceneLoaded(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }
}