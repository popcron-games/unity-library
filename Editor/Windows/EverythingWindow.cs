#nullable enable
using Popcron;
using Popcron.Sealable;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Popcron.Lib.Editor
{
    public class EverythingWindow : SealableEditorWindow
    {
        public const string Title = "Everything";

        private Vector2 scrollPosition;

        [MenuItem("Window/" + Title)]
        public static void ShowWindow()
        {
            GetWindow<EverythingWindow>(Title);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (object obj in Everything.All)
            {
                if (obj is Object unityObject)
                {
                    EditorGUILayout.ObjectField(unityObject, unityObject.GetType(), true);
                }
                else
                {
                    EditorGUILayout.LabelField(obj.ToString());
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
