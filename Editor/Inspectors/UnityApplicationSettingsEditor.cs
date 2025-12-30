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
        private static readonly string[] runtimeSystemsTypeDisplayOptions;
        private static readonly Type[] runtimeSystemsTypeOptions;
        private static readonly string[] editorSystemsTypeDisplayOptions;
        private static readonly Type[] editorSystemsTypeOptions;

        static UnityApplicationSettingsEditor()
        {
            List<string> displayOptions = new();
            List<Type> options = new();

            FindOptions(displayOptions, options, true);
            runtimeSystemsTypeDisplayOptions = displayOptions.ToArray();
            runtimeSystemsTypeOptions = options.ToArray();

            FindOptions(displayOptions, options, false);
            editorSystemsTypeDisplayOptions = displayOptions.ToArray();
            editorSystemsTypeOptions = options.ToArray();
        }

        private static bool SkipAssembly(string assemblyName)
        {
            if (assemblyName == "UnityLibrary.Editor" || assemblyName == "UnityLibrary.Runtime")
            {
                // skip assemblies belonging to this package
                return true;
            }

            if (assemblyName == "mscorlib")
            {
                return true;
            }

            return false;
        }

        private static void FindOptions(List<string> displayOptions, List<Type> options, bool runtimeOnly)
        {
            displayOptions.Clear();
            options.Clear();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<object>())
            {
                if (type.IsPublic && !type.IsAbstract)
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        // unity objects cant be systems
                        continue;
                    }

                    Assembly assembly = type.Assembly;
                    string assemblyName = assembly.GetName().Name;
                    if (SkipAssembly(assemblyName))
                    {
                        continue;
                    }

                    // skip editor types if looking for runtime types only
                    if (runtimeOnly)
                    {
                        if (assemblyName == "Assembly-CSharp-Editor")
                        {
                            continue;
                        }

                        bool isEditor = assembly.GetCustomAttribute<AssemblyIsEditorAssembly>() is not null;
                        if (isEditor)
                        {
                            continue;
                        }
                    }

                    // check if it has a default constructor, or one constructor with vm as parameter
                    bool hasValidConstructor = false;
                    ConstructorInfo[] constructors = type.GetConstructors();
                    foreach (ConstructorInfo constructor in constructors)
                    {
                        ParameterInfo[] parameters = constructor.GetParameters();
                        if (parameters.Length == 0)
                        {
                            hasValidConstructor = true;
                            break;
                        }
                        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(VirtualMachine))
                        {
                            hasValidConstructor = true;
                            break;
                        }
                    }

                    if (!hasValidConstructor)
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
                        displayOptions.Add($"{assemblyName}/{type.Name}");
                        options.Add(type);
                    }
                }
            }
        }

        private SerializedProperty? runtimeSystemsTypeName;
        private SerializedProperty? editorSystemsTypeName;
        private SerializedProperty? addBuiltInSystems;
        private Type? runtimeSystemsType;
        private Type? editorSystemsType;

        private UnityApplicationSettings Settings => (UnityApplicationSettings)target;

        private void OnEnable()
        {
            runtimeSystemsTypeName = serializedObject.FindProperty(UnityApplicationSettings.RuntimeSystemsTypeNameKey);
            editorSystemsTypeName = serializedObject.FindProperty(UnityApplicationSettings.EditorSystemsTypeNameKey);
            addBuiltInSystems = serializedObject.FindProperty(UnityApplicationSettings.AddBuiltInSystemsKey);
            runtimeSystemsType = Settings.RuntimeSystemsType;
            editorSystemsType = Settings.EditorSystemsType;
        }

        public override void OnInspectorGUI()
        {
            if (runtimeSystemsTypeName == null || editorSystemsTypeName == null || addBuiltInSystems == null)
            {
                return;
            }

            serializedObject.Update();
            ShowSystemsProperty(runtimeSystemsTypeName, "Runtime System", runtimeSystemsTypeOptions, runtimeSystemsTypeDisplayOptions);
            ShowSystemsProperty(editorSystemsTypeName, "Editor System (optional)", editorSystemsTypeOptions, editorSystemsTypeDisplayOptions);

            // toggle for disabling built-in systems
            bool changed = false;
            bool boolValue = addBuiltInSystems.boolValue;
            bool newBoolValue = EditorGUILayout.Toggle("Add Built-In Systems", boolValue);
            if (boolValue != newBoolValue)
            {
                addBuiltInSystems.boolValue = newBoolValue;
                changed = true;
            }

            serializedObject.ApplyModifiedProperties();

            // reinitialize if types changed
            if (Settings.RuntimeSystemsType != runtimeSystemsType || Settings.EditorSystemsType != editorSystemsType)
            {
                runtimeSystemsType = Settings.RuntimeSystemsType;
                editorSystemsType = Settings.EditorSystemsType;
                changed = true;
            }

            if (changed)
            {
                UnityApplication.TryReinitialize(Settings);
            }
        }

        private void ShowSystemsProperty(SerializedProperty systemsTypeName, string label, Type[] options, string[] displayOptions)
        {
            bool editManually = EditorPrefs.GetBool(systemsTypeName.propertyPath, false);
            EditorGUILayout.BeginHorizontal();
            Type? editorSystemsType = Type.GetType(systemsTypeName.stringValue);
            if (!editManually)
            {
                int selectedEditorSystemsType = Array.IndexOf(options, editorSystemsType);
                int newEditorSystemsType = EditorGUILayout.Popup(label, selectedEditorSystemsType, displayOptions);
                if (newEditorSystemsType != selectedEditorSystemsType)
                {
                    Type newType = options[newEditorSystemsType];
                    UnityApplicationSettings settings = Settings;
                    systemsTypeName.stringValue = newType.AssemblyQualifiedName;
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssetIfDirty(settings);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(systemsTypeName, new GUIContent(label), GUILayout.ExpandWidth(true));
            }

            // toggle for editing manually
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_editicon.sml"), GUILayout.Width(30)))
            {
                editManually = !editManually;
                EditorPrefs.SetBool(systemsTypeName.propertyPath, editManually);
            }

            EditorGUILayout.EndHorizontal();
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