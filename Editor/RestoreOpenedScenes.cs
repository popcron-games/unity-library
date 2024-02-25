#nullable enable
using System;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityLibrary.Functions
{
    public readonly struct RestoreOpenedScenes
    {
        private const string ScenesBeforePlayKey = "scenesBeforePlay";

        public static void Invoke()
        {
            ReadOnlySpan<char> scenesBeforePlay = EditorPrefs.GetString(ScenesBeforePlayKey);
            int start = 0;
            int length = scenesBeforePlay.Length;
            int position = 0;
            while (position < length)
            {
                char c = scenesBeforePlay[position];
                if (c == ';')
                {
                    ReadOnlySpan<char> scenePath = scenesBeforePlay[start..position];
                    if (scenePath.Length > 0)
                    {
                        EditorSceneManager.OpenScene(scenePath.ToString(), OpenSceneMode.Additive);
                    }

                    start = position + 1;
                }

                position++;
            }

            EditorPrefs.DeleteKey(ScenesBeforePlayKey);
        }
    }
}