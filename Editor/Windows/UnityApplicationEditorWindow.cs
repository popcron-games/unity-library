#nullable enable
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityLibrary.Unity
{
    public class UnityApplicationEditorWindow : EditorWindow
    {
        private const string Title = "Unity Application";

        private bool showSystems;
        private Vector2 scrollPosition;

        private void OnGUI()
        {
            UnityApplicationSettings settings = UnityApplicationSettings.Singleton;
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Settings", settings, typeof(UnityApplicationSettings), false);
            GUI.enabled = true;

            //show error if program type isnt set
            Type? programType = settings.ProgramType;
            if (programType is null)
            {
                EditorGUILayout.HelpBox("No program type is set in the settings", MessageType.Error);
                if (GUILayout.Button("Select settings asset"))
                {
                    Selection.activeObject = settings;
                    EditorGUIUtility.PingObject(settings);
                }

                return;
            }

            //show error if initial data is missing
            if (settings.InitialData == null)
            {
                EditorGUILayout.HelpBox("No initial data was set in the settings", MessageType.Error);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            showSystems = EditorGUILayout.BeginFoldoutHeaderGroup(showSystems, "Systems");
            if (showSystems)
            {
                EditorGUI.indentLevel++;
                foreach (object system in UnityApplication.VM.Systems)
                {
                    if (system is Object unityObject)
                    {
                        EditorGUILayout.ObjectField(unityObject, unityObject.GetType(), true);
                    }
                    else
                    {
                        //todo: for non object systems, show the text asset that declares the system type
                        EditorGUILayout.LabelField(system.ToString());
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Window/Unity Library/Application")]
        public static void Open()
        {
            GetWindow<UnityApplicationEditorWindow>(Title);
        }
    }
}