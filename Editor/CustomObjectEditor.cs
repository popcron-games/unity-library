#nullable enable
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Library.Unity
{
    public abstract class CustomObjectEditor : Editor
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly List<FieldInfo> fields = new();
        private readonly List<PropertyInfo> properties = new();
        private readonly List<MethodInfo> methods = new();
        private readonly Dictionary<string, SerializedProperty> nameToProperty = new();

        public IReadOnlyList<FieldInfo> Fields => fields;
        public IReadOnlyList<PropertyInfo> Properties => properties;
        public IReadOnlyList<MethodInfo> Methods => methods;

        protected virtual void OnEnable()
        {
            fields.AddRange(target.GetType().GetFields(Flags));
            properties.AddRange(target.GetType().GetProperties(Flags));
            methods.AddRange(target.GetType().GetMethods(Flags));
            FindProperties();
        }

        protected virtual void OnDisable()
        {
            fields.Clear();
            properties.Clear();
            methods.Clear();
            nameToProperty.Clear();
        }

        private void FindProperties()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                SerializedProperty property = serializedObject.FindProperty(iterator.name);
                nameToProperty.Add(property.name, property);
                enterChildren = false;
            }
        }

        public void DrawMember(MemberInfo member)
        {
            if (nameToProperty.TryGetValue(member.Name, out SerializedProperty? serializedProperty))
            {
                CustomEditorFunctions.Draw(serializedProperty);
            }
            else
            {
                if (member is FieldInfo field)
                {
                    GUI.enabled = false;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(CustomEditorFunctions.GetDisplayName(field.Name));
                    CustomEditorFunctions.Draw(target, field);
                    EditorGUILayout.EndHorizontal();
                    GUI.enabled = true;
                }
                else if (member is PropertyInfo property)
                {
                    GUI.enabled = false;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(CustomEditorFunctions.GetDisplayName(property.Name));
                    CustomEditorFunctions.Draw(target, property);
                    EditorGUILayout.EndHorizontal();
                    GUI.enabled = true;
                }
            }
        }

        private bool ShouldIgnoreMember(MemberInfo member)
        {
            if (member.DeclaringType == typeof(Object) || member.DeclaringType == typeof(CustomMonoBehaviour) || member.DeclaringType == typeof(CustomScriptableObject))
            {
                return true;
            }
            else if (member.DeclaringType == typeof(Component) || member.DeclaringType == typeof(Behaviour) || member.DeclaringType == typeof(UnityEngine.ScriptableObject))
            {
                return true;
            }
            else if (member.DeclaringType == typeof(object))
            {
                return true;
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            if (target is MonoBehaviour)
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false);
            }
            else if (target is UnityEngine.ScriptableObject)
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((UnityEngine.ScriptableObject)target), typeof(UnityEngine.ScriptableObject), false);
            }

            EditorGUI.EndDisabledGroup();

            if (Fields.Count > 0)
            {
                HashSet<FieldInfo> fieldsToShow = new();
                foreach (FieldInfo field in Fields)
                {
                    if (ShouldIgnoreMember(field)) continue; //ignore fields declared in base classes
                    if (field.Name.EndsWith(">k__BackingField")) continue;

                    fieldsToShow.Add(field);
                }

                if (fieldsToShow.Count > 0)
                {
                    EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);
                    foreach (FieldInfo field in fieldsToShow)
                    {
                        DrawMember(field);
                    }
                }
            }

            if (Properties.Count > 0)
            {
                HashSet<PropertyInfo> propertiesToShow = new();
                foreach (PropertyInfo property in Properties)
                {
                    if (ShouldIgnoreMember(property)) continue;
                    if (property.Name == "VM" && property.PropertyType == typeof(VirtualMachine)) continue;

                    if (property.CanRead && !property.CanWrite)
                    {
                        try
                        {
                            bool show = true;
                            object? value = property.GetValue(target);
                            foreach (FieldInfo field in Fields)
                            {
                                object? fieldValue = field.GetValue(target);
                                if (value == fieldValue)
                                {
                                    show = false; //skip properties that are read only, and their result is the same as an existing field
                                    break;
                                }
                            }

                            if (!show)
                            {
                                continue;
                            }
                        }
                        catch
                        {
                            //unfair to throw exceptions here (not the components fault)
                        }
                    }

                    propertiesToShow.Add(property);
                }

                if (propertiesToShow.Count > 0)
                {
                    EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
                    foreach (PropertyInfo property in propertiesToShow)
                    {
                        DrawMember(property);
                    }
                }
            }

            if (Methods.Count > 0)
            {
                HashSet<MethodInfo> methodsToShow = new();
                foreach (MethodInfo method in Methods)
                {
                    if (ShouldIgnoreMember(method)) continue;
                    if (!method.IsPublic) continue;
                    if (method.Name.StartsWith("get_")) continue;

                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        methodsToShow.Add(method);
                    }
                }

                if (methodsToShow.Count > 0)
                {
                    EditorGUILayout.LabelField("Methods", EditorStyles.boldLabel);
                    foreach (MethodInfo method in methodsToShow)
                    {
                        if (GUILayout.Button(CustomEditorFunctions.GetDisplayName(method.Name)))
                        {
                            method.Invoke(target, null);
                            EditorUtility.SetDirty(target);
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}