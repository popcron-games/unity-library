#nullable enable
using UnityEditor;
using UnityEngine;

namespace Library.Unity
{
    public class HostEditorWindow : EditorWindow
    {
        public const string Title = "Host";

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Systems:");
            EditorGUI.indentLevel++;
            foreach (object system in Host.VirtualMachine.GetSystemsThatAre<object>())
            {
                if (system is Object unityObject)
                {
                    EditorGUILayout.ObjectField(unityObject, unityObject.GetType(), true);
                }
                else
                {
                    EditorGUILayout.LabelField(system.ToString());
                }
            }

            EditorGUI.indentLevel--;
        }

        [MenuItem("Window/Library/Host")]
        public static void Open()
        {
            GetWindow<HostEditorWindow>(Title);
        }
    }
}