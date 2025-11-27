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

            // find all types derived from IProgram
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

            // find all types that can be a system
            foreach (Type type in TypeCache.GetTypesDerivedFrom<object>())
            {
                if (type.IsPublic && !type.IsAbstract)
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    Assembly assembly = type.Assembly;
                    string assemblyName = assembly.GetName().Name;
                    if (assemblyName == "UnityLibrary.Editor" || assemblyName == "UnityLibrary.Runtime")
                    {
                        continue;
                    }

                    bool referencesThisLibrary = false;
                    foreach (AssemblyName? reference in assembly.GetReferencedAssemblies())
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

        private UnityApplicationSettings? settings;
        private SerializedProperty? programTypeName;
        private SerializedProperty? editorSystemsTypeName;
        private SerializedProperty? addBuiltInSystems;
        private bool editProgramTypeNameManually;
        private bool editEditorSystemsTypeNameManually;

        private void OnEnable()
        {
            settings = (UnityApplicationSettings)target;
            programTypeName = serializedObject.FindProperty(UnityApplicationSettings.ProgramTypeNameKey);
            editorSystemsTypeName = serializedObject.FindProperty(UnityApplicationSettings.EditorSystemsTypeNameKey);
            addBuiltInSystems = serializedObject.FindProperty(UnityApplicationSettings.AddBuiltInSystemsKey);
        }

        public override void OnInspectorGUI()
        {
            if (settings == null || programTypeName == null || editorSystemsTypeName == null || addBuiltInSystems == null)
            {
                return;
            }

            serializedObject.Update();

            // show error if program type is missing
            Type? programType = settings.ProgramType;
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IProgram>();
            if (programType is null)
            {
                string typeName = programTypeName.stringValue;
                if (string.IsNullOrEmpty(typeName))
                {
                    if (types.Count == 0)
                    {
                        EditorGUILayout.HelpBox($"No program type assigned. Please create a class that implements {typeof(IProgram).FullName} and assign it here", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"No program type assigned, but implementations have been found to choose from:", MessageType.Error);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox($"Program type `{typeName}` not found, these implementations have been found to choose from:", MessageType.Error);
                }

                EditorGUILayout.Space();
            }

            // show program type enum
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
                        UnityApplication.Reinitialize(settings);
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

            // show editor systems type field
            EditorGUILayout.BeginHorizontal();
            Type? editorSystemsType = Type.GetType(editorSystemsTypeName.stringValue);
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
                        UnityApplication.Reinitialize(settings);
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

            // toggle for disabling built-in systems
            bool boolValue = addBuiltInSystems.boolValue;
            bool newBoolValue = EditorGUILayout.Toggle("Add Built-In Systems", boolValue);
            if (boolValue != newBoolValue)
            {
                addBuiltInSystems.boolValue = newBoolValue;
                VirtualMachine vm = UnityApplication.VM;
                if (newBoolValue)
                {
                    vm.AddSystem(new UnityLibrarySystems(vm));
                }
                else
                {
                    vm.RemoveSystem<UnityLibrarySystems>().Dispose();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        // custom create asset menu item
        [MenuItem("Assets/Create/Unity Application Settings", priority = 200)]
        private static void CreateUnityApplicationSettingsAsset()
        {
            UnityApplicationSettings? singleton = UnityApplicationSettings.FindSingleton();
            if (singleton != null)
            {
                Debug.LogError($"A {nameof(UnityApplicationSettings)} asset already exists. There can only be one instance of this asset", singleton);
            }

            UnityApplicationSettings.singleton = UnityApplicationSettings.CreateSingleton();
        }
    }
}