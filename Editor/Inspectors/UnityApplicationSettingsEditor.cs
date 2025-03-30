#nullable enable
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.Editor
{
    [CustomEditor(typeof(UnityApplicationSettings), true)]
    public class UnityApplicationSettingsEditor : UnityEditor.Editor
    {
        private static readonly string[] programTypeDisplayOptions;
        private static readonly Type[] programTypeOptions;

        static UnityApplicationSettingsEditor()
        {
            List<string> displayOptions = new();
            List<Type> options = new();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<IProgram>())
            {
                if (type.IsPublic)
                {
                    displayOptions.Add($"{type.Assembly.GetName().Name}/{type.Name}");
                    options.Add(type);
                }
            }

            programTypeDisplayOptions = displayOptions.ToArray();
            programTypeOptions = options.ToArray();
        }

        private UnityApplicationSettings settings;
        private bool editProgramTypeNameManually;
        private SerializedProperty programTypeName;
        private SerializedProperty initialData;

        private void OnEnable()
        {
            settings = (UnityApplicationSettings)target;
            programTypeName = serializedObject.FindProperty("programTypeName");
            initialData = serializedObject.FindProperty("initialData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //show error if program type is missing
            Type? programType = settings.ProgramType;
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IProgram>();
            if (programType is null)
            {
                string typeName = programTypeName.stringValue;
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

                EditorGUILayout.Space();
            }

            //show program type enum
            EditorGUILayout.BeginHorizontal();
            if (!editProgramTypeNameManually)
            {
                int selectedProgramType = Array.IndexOf(programTypeOptions, programType);
                int newProgramType = EditorGUILayout.Popup("Program Type", selectedProgramType, programTypeDisplayOptions);
                if (newProgramType != selectedProgramType)
                {
                    Type newType = programTypeOptions[newProgramType];
                    if (settings.TryAssignProgramType(newType))
                    {
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssetIfDirty(settings);
                        UnityApplication.Reinitialize();
                    }
                }
            }
            else
            {
                EditorGUILayout.PropertyField(programTypeName, new GUIContent("Program Type"), GUILayout.ExpandWidth(true));
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_editicon.sml"), GUILayout.Width(30)))
            {
                editProgramTypeNameManually = !editProgramTypeNameManually;
            }

            EditorGUILayout.EndHorizontal();

            //show initial data
            bool initialDataIsMissing = initialData.objectReferenceValue == null;
            if (initialDataIsMissing)
            {
                GUI.color = Color.red;
            }

            EditorGUILayout.PropertyField(initialData, new GUIContent("Initial Data"));
            if (initialDataIsMissing)
            {
                GUI.color = Color.white;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}