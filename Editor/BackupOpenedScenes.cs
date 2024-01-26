#nullable enable
using System.Text;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Library.Functions
{
    public readonly struct BackupOpenedScenes
    {
        private const string ScenesBeforePlayKey = "scenesBeforePlay";

        public static void Invoke()
        {
            StringBuilder builder = new();
            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!string.IsNullOrEmpty(scene.path))
                {
                    builder.Append(scene.path);
                    builder.Append(';');
                }
            }

            EditorPrefs.SetString(ScenesBeforePlayKey, builder.ToString());
        }
    }
}