#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityLibrary.Drawers;
using Object = UnityEngine.Object;

namespace UnityLibrary.Editor
{
    public abstract class CustomObjectEditor : UnityEditor.Editor
    {
        private const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private static Container? container;
        private static SerializedObject? containerObject;

        private readonly List<FieldInfo> fields = new();
        private readonly List<PropertyInfo> properties = new();
        private readonly List<MethodInfo> methods = new();

        public IReadOnlyList<FieldInfo> Fields => fields;
        public IReadOnlyList<PropertyInfo> Properties => properties;
        public IReadOnlyList<MethodInfo> Methods => methods;

        protected virtual void OnEnable()
        {
            fields.AddRange(target.GetType().GetFields(MemberFlags));
            properties.AddRange(target.GetType().GetProperties(MemberFlags));
            methods.AddRange(target.GetType().GetMethods(MemberFlags));

            //remove members with HideInInspector attribute
            for (int i = fields.Count - 1; i >= 0; i--)
            {
                if (ShouldIgnoreMember(fields[i]))
                {
                    fields.RemoveAt(i);
                }
            }

            for (int i = properties.Count - 1; i >= 0; i--)
            {
                if (ShouldIgnoreMember(properties[i]))
                {
                    properties.RemoveAt(i);
                }
            }

            for (int i = methods.Count - 1; i >= 0; i--)
            {
                if (ShouldIgnoreMember(methods[i]))
                {
                    methods.RemoveAt(i);
                }
            }
        }

        protected virtual void OnDisable()
        {
            fields.Clear();
            properties.Clear();
            methods.Clear();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //show what script is being inspected
            EditorGUI.BeginDisabledGroup(true);
            if (target is MonoBehaviour monoBehaviour)
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(monoBehaviour), typeof(MonoBehaviour), false);
            }
            else if (target is ScriptableObject scriptableObject)
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(scriptableObject), typeof(ScriptableObject), false);
            }

            EditorGUI.EndDisabledGroup();

            //show fields
            foreach (FieldInfo field in Fields)
            {
                DrawMember(field);
            }

            //show properties
            if (Properties.Count > 0)
            {
                foreach (PropertyInfo property in Properties)
                {
                    try
                    {
                        bool show = true;

                        //skip properties that have the same value as a field (likely backing field)
                        object? value = property.GetValue(target);
                        foreach (FieldInfo field in Fields)
                        {
                            object? fieldValue = field.GetValue(target);
                            if (value == fieldValue)
                            {
                                show = false;
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

                    DrawMember(property);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void DrawMember(MemberInfo member)
        {
            bool readOnly = false;
            object? memberValue;
            Type memberType;
            FieldInfo? field = member as FieldInfo;
            PropertyInfo? property = member as PropertyInfo;
            if (field is not null)
            {
                memberType = field.FieldType;
                memberValue = field.GetValue(target);

                //dont show private non serialized fields in edit mode
                bool serialized = field.GetCustomAttribute<SerializeField>() is not null;
                if (!serialized && !EditorApplication.isPlaying && !field.IsPublic)
                {
                    return;
                }

                readOnly = !serialized;
            }
            else if (property is not null)
            {
                memberType = property.PropertyType;
                try
                {
                    memberValue = property.GetValue(target);
                }
                catch (Exception ex)
                {
                    memberValue = ex;
                }

                //dont show properties when in edit mode
                if (!EditorApplication.isPlaying)
                {
                    return;
                }

                readOnly = !property.CanWrite;
            }
            else
            {
                throw new NotImplementedException($"Member type {member.GetType()} is not supported");
            }

            GUI.enabled = !readOnly;
            GUIContent label = new(GetDisplayName(member));
            if (memberValue is Exception exception)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                EditorGUILayout.HelpBox(exception.Message, MessageType.Error);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                memberValue = ManuallyDraw(label, memberType, memberValue);

                if (field is not null)
                {
                    field.SetValue(target, memberValue);
                }
                else if (property is not null)
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(target, memberValue);
                    }
                }
            }

            GUI.enabled = true;
        }

        private static bool ShouldIgnoreMember(MemberInfo member)
        {
            if (member.GetCustomAttribute<HideInInspector>() is not null)
            {
                return true;
            }

            if (member is PropertyInfo property)
            {
                if (!property.CanRead)
                {
                    return false;
                }

                if (property.Name == "VM" && property.PropertyType == typeof(VirtualMachine))
                {
                    return true;
                }
                else
                {
                    if (member.DeclaringType == typeof(Object) || member.DeclaringType == typeof(CustomMonoBehaviour) || member.DeclaringType == typeof(CustomScriptableObject))
                    {
                        return true;
                    }
                    else if (member.DeclaringType == typeof(Component) || member.DeclaringType == typeof(MonoBehaviour) || member.DeclaringType == typeof(Behaviour) || member.DeclaringType == typeof(ScriptableObject))
                    {
                        return true;
                    }
                }
            }
            else if (member is FieldInfo field)
            {
                if (field.Name.EndsWith(">k__BackingField"))
                {
                    return true;
                }
            }

            if (member.DeclaringType == typeof(object))
            {
                return true;
            }

            return false;
        }

        private static string GetDisplayName(MemberInfo member)
        {
            if (member.GetCustomAttribute<InspectorNameAttribute>() is InspectorNameAttribute customName)
            {
                return customName.displayName;
            }

            ReadOnlySpan<char> name = member.Name;
            StringBuilder builder = new();
            bool lastWasUpper = false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i == 0)
                {
                    builder.Append(char.ToUpper(c));
                }
                else if (char.IsUpper(c))
                {
                    if (!lastWasUpper)
                    {
                        builder.Append(' ');
                    }

                    builder.Append(c);
                }
                else
                {
                    builder.Append(c);
                }

                lastWasUpper = char.IsUpper(c);
            }

            return builder.ToString();
        }

        private static (FieldInfo field, object targetObject) GetMember(SerializedProperty property)
        {
            string path = property.propertyPath;
            object targetObject = property.serializedObject.targetObject;
            Type targetType = targetObject.GetType();
            if (targetType.GetField(path, MemberFlags) is FieldInfo field)
            {
                return (field, targetObject);
            }
            else
            {
                throw new NotImplementedException($"Member {path} not found in {targetType}");
            }
        }

        private static object? ManuallyDraw(GUIContent label, Type type, object? value)
        {
            if (container == null)
            {
                container = CreateInstance<Container>();
            }

            if (containerObject is null)
            {
                containerObject = new SerializedObject(container);
            }

            if (MemberDrawers.TryGet(type, out IMemberDrawer? drawer))
            {
                float height = drawer.GetPropertyHeight(value, label);
                Rect position = EditorGUILayout.GetControlRect(false, height);
                try
                {
                    return drawer.OnGUI(position, value, label);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (type.IsArray)
            {
                Array? array = value as Array;
                if (array is null)
                {
                    EditorGUILayout.LabelField("null");
                    return null;
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(label, GUILayout.Width(50));
                    int size = EditorGUILayout.IntField(array.Length);
                    EditorGUILayout.EndHorizontal();
                    Type elementType = type.GetElementType();
                    if (size != array.Length)
                    {
                        if (size > array.Length)
                        {
                            Array newArray = Array.CreateInstance(elementType, size);
                            Array.Copy(array, newArray, array.Length);
                            array = newArray;
                        }
                        else
                        {
                            Array newArray = Array.CreateInstance(elementType, size);
                            Array.Copy(array, newArray, size);
                            array = newArray;
                        }
                    }

                    EditorGUI.indentLevel++;
                    for (int i = 0; i < array.Length; i++)
                    {
                        object? element = array.GetValue(i);
                        GUIContent elementContent = new($"Element {i}");
                        element = ManuallyDraw(elementContent, elementType, element);
                        array.SetValue(element, i);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    return array;
                }
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                IList list;
                if (value is null)
                {
                    EditorGUILayout.LabelField("null");
                    return null;
                }
                else
                {
                    list = (IList)value;
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(50));
                int size = EditorGUILayout.IntField(list.Count);
                Type elementType = type.GetGenericArguments()[0];
                EditorGUILayout.EndHorizontal();
                if (size != list.Count)
                {
                    if (size > list.Count)
                    {
                        while (list.Count < size)
                        {
                            list.Add(null);
                        }
                    }
                    else
                    {
                        while (list.Count > size)
                        {
                            list.RemoveAt(list.Count - 1);
                        }
                    }
                }

                EditorGUI.indentLevel++;
                for (int i = 0; i < list.Count; i++)
                {
                    object? element = list[i];
                    GUIContent elementContent = new($"Element {i}");
                    list[i] = ManuallyDraw(elementContent, elementType, element);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                return list;
            }
            else if (value is UnityEventBase unityEventBase)
            {
                //container.unityEventValue = unityEventBase;
                //SerializedProperty property = containerObject.FindProperty("unityEventValue");
                //EditorGUILayout.PropertyField(property);
                //return container.unityEventValue;
                return unityEventBase;
            }
            else if (value is Enum enumValue)
            {
                return EditorGUILayout.EnumPopup(label, enumValue);
            }
            else if (value is bool boolValue)
            {
                container.boolValue = boolValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("boolValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.boolValue;
            }
            else if (value is int intValue)
            {
                container.intValue = intValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("intValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.intValue;
            }
            else if (value is long longValue)
            {
                container.longValue = longValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("longValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.longValue;
            }
            else if (value is float floatValue)
            {
                container.floatValue = floatValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("floatValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.floatValue;
            }
            else if (value is double doubleValue)
            {
                container.doubleValue = doubleValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("doubleValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.doubleValue;
            }
            else if (type == typeof(string))
            {
                container.stringValue = value as string ?? string.Empty;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("stringValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.stringValue;
            }
            else if (value is Vector2 vector2Value)
            {
                container.vector2Value = vector2Value;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("vector2Value");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.vector2Value;
            }
            else if (value is Vector3 vector3Value)
            {
                container.vector3Value = vector3Value;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("vector3Value");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.vector3Value;
            }
            else if (value is Vector4 vector4Value)
            {
                container.vector4Value = vector4Value;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("vector4Value");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.vector4Value;
            }
            else if (value is Quaternion quaternionValue)
            {
                container.quaternionValue = quaternionValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("quaternionValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.quaternionValue;
            }
            else if (value is Color colorValue)
            {
                container.colorValue = colorValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("colorValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.colorValue;
            }
            else if (value is Rect rectValue)
            {
                container.rectValue = rectValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("rectValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.rectValue;
            }
            else if (value is Bounds boundsValue)
            {
                container.boundsValue = boundsValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("boundsValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.boundsValue;
            }
            else if (type == typeof(AnimationCurve))
            {
                container.animationCurveValue = value as AnimationCurve ?? new();
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("animationCurveValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.animationCurveValue;
            }
            else if (type == typeof(Gradient))
            {
                container.gradientValue = value as Gradient ?? new();
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("gradientValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                return container.gradientValue;
            }
            else if (typeof(Object).IsAssignableFrom(type))
            {
                return EditorGUILayout.ObjectField(label, value as Object, type, true);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                EditorGUILayout.HelpBox($"Type {type} not supported", MessageType.Error);
                EditorGUILayout.EndHorizontal();
                return value;
            }
        }

        public sealed class Container : ScriptableObject
        {
            public bool boolValue;
            public float floatValue;
            public int intValue;
            public long longValue;
            public double doubleValue;
            public string stringValue;
            public Vector2 vector2Value;
            public Vector3 vector3Value;
            public Vector4 vector4Value;
            public Quaternion quaternionValue;
            public Color colorValue;
            public Rect rectValue;
            public Bounds boundsValue;
            public AnimationCurve animationCurveValue;
            public Gradient gradientValue;
            public UnityEventBase unityEventValue;
        }

        public sealed class UnityEventContainer<T1> : ScriptableObject
        {
            public UnityEvent<T1> value;
        }

        public sealed class UnityEventContainer<T1, T2> : ScriptableObject
        {
            public UnityEvent<T1, T2> value;
        }

        public sealed class UnityEventContainer<T1, T2, T3> : ScriptableObject
        {
            public UnityEvent<T1, T2, T3> value;
        }

        public sealed class UnityEventContainer<T1, T2, T3, T4> : ScriptableObject
        {
            public UnityEvent<T1, T2, T3, T4> value;
        }
    }
}