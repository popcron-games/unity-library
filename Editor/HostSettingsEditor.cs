#nullable enable
using System;
using UnityEditor;
using UnityEngine;

namespace Library.Unity
{
    [CustomEditor(typeof(HostSettings), true)]
    public class HostSettingsEditor : Editor
    {
        private SerializedProperty stateTypeName;
        private SerializedProperty assets;
        private HostSettings settings;

        private void OnEnable()
        {
            settings = (HostSettings)target;
            stateTypeName = serializedObject.FindProperty("stateTypeName");
            assets = serializedObject.FindProperty("assets");
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
            if (stateType is null)
            {
                string typeName = stateTypeName.stringValue;
                TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IState>();
                if (string.IsNullOrEmpty(typeName))
                {
                    if (types.Count == 0)
                    {
                        EditorGUILayout.HelpBox($"No state type assigned. Please create a class that implements {typeof(IState).FullName} and assign it here.", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"No state type assigned, but implementations have been found to choose from:", MessageType.Error);
                        foreach (Type type in types)
                        {
                            if (GUILayout.Button(type.FullName))
                            {
                                stateTypeName.stringValue = type.AssemblyQualifiedName;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox($"State type {typeName} not found, these implementations have been found to choose from:", MessageType.Error);
                    foreach (Type type in types)
                    {
                        if (GUILayout.Button(type.FullName))
                        {
                            stateTypeName.stringValue = type.AssemblyQualifiedName;
                            break;
                        }
                    }
                }
            }
        }
    }
}