#nullable enable
using System;
using Game;
using UnityEditor;
using UnityEngine;
using UnityLibrary.Systems;
using Object = UnityEngine.Object;

namespace UnityLibrary.Unity
{
    public class UnityApplicationEditorWindow : EditorWindow
    {
        public const string Title = "Unity Application";

        private bool showSystems;
        private bool changeType;
        private bool changeInitialData;
        private Vector2 scrollPosition;

        private void OnGUI()
        {
            UnityApplicationSettings settings = UnityApplicationSettings.Singleton;
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Settings", settings, typeof(UnityApplicationSettings), false);
            GUI.enabled = true;
            if (settings.StateType is null)
            {
                EditorGUILayout.HelpBox("No state type was set in the settings.", MessageType.Error);
                if (GUILayout.Button("Select root settings asset"))
                {
                    Selection.activeObject = settings;
                    EditorGUIUtility.PingObject(settings);
                }

                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("State Type", settings.StateType.Name);
            changeType = EditorGUILayout.BeginFoldoutHeaderGroup(changeType, "Change State Type");
            if (changeType)
            {
                EditorGUI.indentLevel++;
                TypeCache.TypeCollection stateTypes = TypeCache.GetTypesDerivedFrom<VirtualMachine.IState>();
                foreach (Type type in stateTypes)
                {
                    if (!type.IsPublic) continue;

                    string assemblyName = type.Assembly.GetName().Name;
                    if (GUILayout.Button($"{type.FullName} from {assemblyName}"))
                    {
                        if (settings.AssignStateType(type))
                        {
                            EditorUtility.SetDirty(settings);
                            AssetDatabase.SaveAssetIfDirty(settings);
                            UnityApplication.Reinitialize();
                        }

                        break;
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            if (settings.InitialData == null)
            {
                EditorGUILayout.HelpBox("No initial data was set in the settings.", MessageType.Error);
                if (GUILayout.Button("Select root settings asset"))
                {
                    Selection.activeObject = settings;
                    EditorGUIUtility.PingObject(settings);
                }

                return;
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Initial Data", settings.InitialData, settings.InitialData.GetType(), false);
            GUI.enabled = true;
            changeInitialData = EditorGUILayout.BeginFoldoutHeaderGroup(changeInitialData, "Change Initial Data");
            if (changeInitialData)
            {
                EditorGUI.indentLevel++;
                string[] guids = AssetDatabase.FindAssets($"t:{typeof(InitialAssets).FullName}");
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    InitialAssets initialAssets = AssetDatabase.LoadAssetAtPath<InitialAssets>(path);
                    if (GUILayout.Button(initialAssets.name))
                    {
                        if (settings.AssignInitialData(initialAssets))
                        {
                            EditorUtility.SetDirty(settings);
                            AssetDatabase.SaveAssetIfDirty(settings);
                            UnityApplication.Reinitialize();
                        }
                        break;
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            showSystems = EditorGUILayout.BeginFoldoutHeaderGroup(showSystems, "Systems");
            if (showSystems)
            {
                EditorGUI.indentLevel++;
                foreach (object system in UnityApplication.VM.GetSystemsThatAre<object>())
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
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndScrollView();
        }

        [MenuItem(nameof(UnityLibrary) + "/" + Title)]
        public static void Open()
        {
            GetWindow<UnityApplicationEditorWindow>(Title);
        }
    }
}