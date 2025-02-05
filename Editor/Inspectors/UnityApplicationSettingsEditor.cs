#nullable enable
using System;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.Editor
{
    [CustomEditor(typeof(UnityApplicationSettings), true)]
    public class UnityApplicationSettingsEditor : UnityEditor.Editor
    {
        private UnityApplicationSettings settings;

        private void OnEnable()
        {
            settings = (UnityApplicationSettings)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            ValidateStateType();
            serializedObject.ApplyModifiedProperties();
        }

        private void ValidateStateType()
        {
            Type? stateType = settings.StateType;
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IProgram>();
            if (stateType is null)
            {
                SerializedProperty stateTypeName = serializedObject.FindProperty("stateTypeName");
                string typeName = stateTypeName.stringValue;
                if (string.IsNullOrEmpty(typeName))
                {
                    if (types.Count == 0)
                    {
                        EditorGUILayout.HelpBox($"No state type assigned. Please create a class that implements {typeof(IProgram).FullName} and assign it here.", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"No state type assigned, but implementations have been found to choose from:", MessageType.Error);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox($"State type {typeName} not found, these implementations have been found to choose from:", MessageType.Error);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Available state types", EditorStyles.boldLabel);

            foreach (Type type in types)
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

            EditorGUILayout.LabelField("Available initial assets", EditorStyles.boldLabel);
            string[] guids = AssetDatabase.FindAssets("t:UnityLibrary.InitialAssets");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InitialAssets assets = AssetDatabase.LoadAssetAtPath<InitialAssets>(path);
                if (GUILayout.Button(assets.name))
                {
                    if (settings.AssignInitialData(assets))
                    {
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssetIfDirty(settings);
                        UnityApplication.Reinitialize();
                    }

                    break;
                }
            }
        }
    }
}