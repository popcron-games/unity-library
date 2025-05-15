#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.Editor
{
    [CustomEditor(typeof(UnityApplicationSettings), true)]
    public class UnityApplicationSettingsEditor : UnityEditor.Editor
    {
        private static readonly string[] programTypeDisplayOptions;
        private static readonly Type[] programTypeOptions;
        private static readonly string[] editorSystemsTypeDisplayOptions;
        private static readonly Type[] editorSystemsTypeOptions;

        static UnityApplicationSettingsEditor()
        {
            List<string> displayOptions = new();
            List<Type> options = new();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<IProgram>())
            {
                if (type.IsPublic && !type.IsAbstract)
                {
                    displayOptions.Add($"{type.Assembly.GetName().Name}/{type.Name}");
                    options.Add(type);
                }
            }

            programTypeDisplayOptions = displayOptions.ToArray();
            programTypeOptions = options.ToArray();
            displayOptions.Clear();
            options.Clear();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<object>())
            {
                if (type.IsPublic && !type.IsAbstract)
                {
                    bool referencesThisLibrary = false;
                    foreach (AssemblyName? reference in type.Assembly.GetReferencedAssemblies())
                    {
                        if (reference.Name == "UnityLibrary")
                        {
                            referencesThisLibrary = true;
                            break;
                        }
                    }

                    if (referencesThisLibrary)
                    {
                        displayOptions.Add($"{type.Assembly.GetName().Name}/{type.Name}");
                        options.Add(type);
                    }
                }
            }

            editorSystemsTypeDisplayOptions = displayOptions.ToArray();
            editorSystemsTypeOptions = options.ToArray();
        }

        private UnityApplicationSettings settings;
        private bool editProgramTypeNameManually;
        private bool editEditorSystemsTypeNameManually;
        private SerializedProperty programTypeName;
        private SerializedProperty editorSystemsTypeName;

        private void OnEnable()
        {
            settings = (UnityApplicationSettings)target;
            programTypeName = serializedObject.FindProperty(UnityApplicationSettings.ProgramTypeName);
            editorSystemsTypeName = serializedObject.FindProperty(UnityApplicationSettings.EditorSystemsTypeName);
        }

        private Type? GetEditorSystemsType()
        {
            return Type.GetType(editorSystemsTypeName.stringValue);
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

            //show editor systems type field
            EditorGUILayout.BeginHorizontal();
            Type? editorSystemsType = GetEditorSystemsType();
            if (!editEditorSystemsTypeNameManually)
            {
                int selectedEditorSystemsType = Array.IndexOf(editorSystemsTypeOptions, editorSystemsType);
                int newEditorSystemsType = EditorGUILayout.Popup("Editor Systems Type", selectedEditorSystemsType, editorSystemsTypeDisplayOptions);
                if (newEditorSystemsType != selectedEditorSystemsType)
                {
                    Type newType = editorSystemsTypeOptions[newEditorSystemsType];
                    if (settings.TryAssignEditorSystemsType(newType))
                    {
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssetIfDirty(settings);
                        UnityApplication.Reinitialize();
                    }
                }
            }
            else
            {
                EditorGUILayout.PropertyField(editorSystemsTypeName, new GUIContent("Editor Systems Type"), GUILayout.ExpandWidth(true));
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_editicon.sml"), GUILayout.Width(30)))
            {
                editEditorSystemsTypeNameManually = !editEditorSystemsTypeNameManually;
            }

            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }
    }
}